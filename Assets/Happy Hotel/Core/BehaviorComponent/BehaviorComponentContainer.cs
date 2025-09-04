using System;
using System.Collections.Generic;
using System.Linq;
using HappyHotel.Core.ValueProcessing.Components;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HappyHotel.Core.BehaviorComponent
{
    // 组件容器，管理游戏对象上的所有组件
    [AutoInitComponent(typeof(ProcessableValueCollectorBehaviorComponent))]
    public class BehaviorComponentContainer : SerializedMonoBehaviour, IComponentContainer
    {
        // 存储所有组件的字典
        private Dictionary<Type, IBehaviorComponent> components = new();

        // 延迟添加的组件队列
        private Queue<IBehaviorComponent> pendingComponents = new();

        // 延迟移除的组件类型队列
        private Queue<Type> pendingRemovals = new();

        // 标记是否正在遍历组件（防止在遍历过程中修改集合）
        private bool isIteratingComponents;

        // Tag系统 - 直接存储标签
        [SerializeField] private HashSet<string> tags = new();

        public IBehaviorComponent AddBehaviorComponent(IBehaviorComponent component)
        {
            if (component == null)
                throw new ArgumentNullException(nameof(component));

            // 如果正在遍历组件，将添加操作延迟到遍历结束后
            if (isIteratingComponents)
            {
                pendingComponents.Enqueue(component);
                return component;
            }

            return AddBehaviorComponentInternal(component);
        }

        // 内部添加组件方法
        private IBehaviorComponent AddBehaviorComponentInternal(IBehaviorComponent component)
        {
            var type = component.GetType();

            // 如果组件已存在，先移除
            if (components.ContainsKey(type))
            {
                var existingComponent = components[type];
                existingComponent.OnDetach();
                components.Remove(type);
            }

            // 检查组件依赖
            var attributes = type.GetCustomAttributes(typeof(DependsOnComponentAttribute), true);
            foreach (DependsOnComponentAttribute attr in attributes)
                if (attr.AutoAdd && !HasBehaviorComponentOfType(attr.RequiredType))
                    try
                    {
                        // 创建依赖组件实例
                        var dependencyComponent = Activator.CreateInstance(attr.RequiredType) as IBehaviorComponent;
                        if (dependencyComponent != null) AddBehaviorComponentInternal(dependencyComponent);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"创建依赖组件 {attr.RequiredType.Name} 失败: {e.Message}");
                        throw;
                    }

            // 添加组件
            components[type] = component;
            component.OnAttach(this);

            return component;
        }

        // 修改泛型版本的AddBehaviorComponent方法以使用非泛型版本
        public T AddBehaviorComponent<T>() where T : class, IBehaviorComponent, new()
        {
            var type = typeof(T);

            // 如果组件已存在，直接返回
            if (components.TryGetValue(type, out var existingComponent)) return existingComponent as T;

            // 创建新组件实例并添加
            var component = new T();
            return AddBehaviorComponent(component) as T;
        }

        // 修改InitializeRequiredComponents方法以使用非泛型版本
        protected virtual void InitializeRequiredComponents()
        {
            // 保留原有的Attribute方式（向后兼容）
            var attributes = GetType().GetCustomAttributes(typeof(AutoInitComponentAttribute), true);

            foreach (AutoInitComponentAttribute attr in attributes)
                // 检查组件是否已存在
                if (!HasBehaviorComponentOfType(attr.ComponentType))
                    try
                    {
                        // 创建组件实例
                        var component = Activator.CreateInstance(attr.ComponentType) as IBehaviorComponent;

                        // 调用相应的初始化方法(如果有参数)
                        if (attr.InitParams.Length > 0 && component != null)
                        {
                            // 查找合适的初始化方法并调用
                            var initMethod = attr.ComponentType.GetMethod("Initialize");
                            if (initMethod != null)
                                initMethod.Invoke(component, attr.InitParams);
                            else
                                Debug.LogWarning($"组件 {attr.ComponentType.Name} 没有找到Initialize方法，但提供了初始化参数");
                        }

                        // 添加组件到容器
                        if (component != null)
                        {
                            AddBehaviorComponentInternal(component);
                            Debug.Log($"自动初始化组件: {attr.ComponentType.Name}");
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"初始化组件 {attr.ComponentType.Name} 失败: {e.Message}\n{e.StackTrace}");
                    }

            // 新增：使用注册器初始化组件
            ComponentInitializerRegistry.InitializeComponents(this);
        }

        // 获取组件
        public T GetBehaviorComponent<T>() where T : class, IBehaviorComponent
        {
            var type = typeof(T);

            // 直接匹配类型
            if (components.TryGetValue(type, out var component)) return component as T;

            // 检查接口实现
            foreach (var pair in components)
                if (typeof(T).IsAssignableFrom(pair.Key))
                    return pair.Value as T;

            return null;
        }

        // 移除组件
        public bool RemoveBehaviorComponent<T>() where T : class, IBehaviorComponent
        {
            var type = typeof(T);

            // 如果正在遍历组件，将移除操作延迟到遍历结束后
            if (isIteratingComponents)
            {
                pendingRemovals.Enqueue(type);
                return true; // 假设移除会成功，实际结果在延迟处理时确定
            }

            return RemoveBehaviorComponentInternal(type);
        }

        // 内部移除组件方法
        private bool RemoveBehaviorComponentInternal(Type type)
        {
            if (components.TryGetValue(type, out var component))
            {
                // 检查是否有其他组件依赖于此组件
                foreach (var pair in components)
                {
                    var attributes = pair.Key.GetCustomAttributes(typeof(DependsOnComponentAttribute), true);
                    foreach (DependsOnComponentAttribute attr in attributes)
                        if (attr.RequiredType == type)
                        {
                            Debug.LogWarning($"无法移除组件 {type.Name}，因为组件 {pair.Key.Name} 依赖于它");
                            return false;
                        }
                }

                component.OnDetach();
                components.Remove(type);

                Debug.Log($"移除 {gameObject.name} 的组件 {type.Name}");

                return true;
            }

            return false;
        }

        // 处理延迟的组件操作
        private void ProcessPendingOperations()
        {
            // 处理延迟添加的组件
            while (pendingComponents.Count > 0)
            {
                var component = pendingComponents.Dequeue();
                AddBehaviorComponentInternal(component);
            }

            // 处理延迟移除的组件
            while (pendingRemovals.Count > 0)
            {
                var type = pendingRemovals.Dequeue();
                RemoveBehaviorComponentInternal(type);
            }
        }

        // 查询组件是否存在
        public bool HasBehaviorComponent<T>() where T : class, IBehaviorComponent
        {
            return GetBehaviorComponent<T>() != null;
        }

        // 检查是否存在指定类型的组件
        public bool HasBehaviorComponentOfType(Type type)
        {
            if (components.ContainsKey(type))
                return true;

            foreach (var pair in components)
                if (type.IsAssignableFrom(pair.Key))
                    return true;

            return false;
        }

        // 获取所有实现特定接口的组件
        public List<T> GetBehaviorComponents<T>() where T : class
        {
            return components.Values
                .OfType<T>()
                .ToList();
        }

        // 发送事件到所有组件
        public void SendEvent(BehaviorComponentEvent evt)
        {
            // 创建组件列表的副本以避免在遍历过程中集合被修改
            var componentsCopy = components.Values.ToList();

            isIteratingComponents = true;
            try
            {
                foreach (var component in componentsCopy)
                    if (component is IEventListener listener && listener.IsEnabled)
                        listener.OnEvent(evt);
            }
            finally
            {
                isIteratingComponents = false;
                // 处理在事件处理过程中延迟的操作
                ProcessPendingOperations();
            }
        }

        // 发送事件到指定类型的组件
        public void SendEvent<T>(BehaviorComponentEvent evt) where T : class, IEventListener
        {
            var listeners = GetBehaviorComponents<T>();

            isIteratingComponents = true;
            try
            {
                foreach (var listener in listeners)
                    if (listener.IsEnabled)
                        listener.OnEvent(evt);
            }
            finally
            {
                isIteratingComponents = false;
                // 处理在事件处理过程中延迟的操作
                ProcessPendingOperations();
            }
        }

        #region 标签管理

        // 添加标签
        public void AddTag(string tag)
        {
            if (!string.IsNullOrEmpty(tag)) tags.Add(tag);
        }

        // 移除标签
        public void RemoveTag(string tag)
        {
            if (!string.IsNullOrEmpty(tag)) tags.Remove(tag);
        }

        // 检查是否包含标签
        public bool HasTag(string tag)
        {
            return !string.IsNullOrEmpty(tag) && tags.Contains(tag);
        }

        // 检查是否包含任意一个标签
        public bool HasAnyTag(IEnumerable<string> targetTags)
        {
            return targetTags != null && targetTags.Any(tag => HasTag(tag));
        }

        // 检查是否包含所有标签
        public bool HasAllTags(IEnumerable<string> targetTags)
        {
            return targetTags != null && targetTags.All(tag => HasTag(tag));
        }

        // 获取所有标签
        public IReadOnlyCollection<string> GetTags()
        {
            return tags;
        }

        #endregion

        #region IComponentContainer实现

        // 容器名称
        public string Name
        {
            get
            {
                // 在构造函数阶段，gameObject可能还未初始化，返回默认名称
                try
                {
                    if (this == null || gameObject == null)
                        return "BehaviorContainer";

                    return gameObject.name;
                }
                catch (Exception)
                {
                    return "BehaviorContainer";
                }
            }
        }

        // 获取GameObject
        public GameObject GetGameObject()
        {
            return gameObject;
        }

        // 检查容器是否有效
        public bool IsValid => this != null && gameObject != null;

        #endregion

        #region Unity Events

        protected virtual void Awake()
        {
            // 创建组件列表的副本以避免在遍历过程中集合被修改
            var componentsCopy = components.Values.ToList();

            isIteratingComponents = true;
            try
            {
                foreach (var component in componentsCopy)
                    if (component is BehaviorComponentBase behaviorComponent && behaviorComponent.IsEnabled)
                        behaviorComponent.OnAwake();
            }
            finally
            {
                isIteratingComponents = false;
                ProcessPendingOperations();
            }

            // 初始化通过Attribute标记的必需组件
            InitializeRequiredComponents();
        }

        protected virtual void Start()
        {
            var componentsCopy = components.Values.ToList();

            isIteratingComponents = true;
            try
            {
                foreach (var component in componentsCopy)
                    if (component is BehaviorComponentBase behaviorComponent && behaviorComponent.IsEnabled)
                        behaviorComponent.OnStart();
            }
            finally
            {
                isIteratingComponents = false;
                ProcessPendingOperations();
            }
        }

        protected virtual void Update()
        {
            var componentsCopy = components.Values.ToList();

            isIteratingComponents = true;
            try
            {
                foreach (var component in componentsCopy)
                    if (component is BehaviorComponentBase behaviorComponent && behaviorComponent.IsEnabled)
                        behaviorComponent.OnUpdate();
            }
            finally
            {
                isIteratingComponents = false;
                ProcessPendingOperations();
            }
        }

        protected virtual void FixedUpdate()
        {
            var componentsCopy = components.Values.ToList();

            isIteratingComponents = true;
            try
            {
                foreach (var component in componentsCopy)
                    if (component is BehaviorComponentBase behaviorComponent && behaviorComponent.IsEnabled)
                        behaviorComponent.OnFixedUpdate();
            }
            finally
            {
                isIteratingComponents = false;
                ProcessPendingOperations();
            }
        }

        protected virtual void LateUpdate()
        {
            var componentsCopy = components.Values.ToList();

            isIteratingComponents = true;
            try
            {
                foreach (var component in componentsCopy)
                    if (component is BehaviorComponentBase behaviorComponent && behaviorComponent.IsEnabled)
                        behaviorComponent.OnLateUpdate();
            }
            finally
            {
                isIteratingComponents = false;
                ProcessPendingOperations();
            }
        }

        protected virtual void OnEnable()
        {
            var componentsCopy = components.Values.ToList();

            isIteratingComponents = true;
            try
            {
                foreach (var component in componentsCopy)
                    if (component is BehaviorComponentBase behaviorComponent && behaviorComponent.IsEnabled)
                        behaviorComponent.OnEnable();
            }
            finally
            {
                isIteratingComponents = false;
                ProcessPendingOperations();
            }
        }

        protected virtual void OnDisable()
        {
            var componentsCopy = components.Values.ToList();

            isIteratingComponents = true;
            try
            {
                foreach (var component in componentsCopy)
                    if (component is BehaviorComponentBase behaviorComponent && behaviorComponent.IsEnabled)
                        behaviorComponent.OnDisable();
            }
            finally
            {
                isIteratingComponents = false;
                ProcessPendingOperations();
            }
        }

        protected virtual void OnDestroy()
        {
            var componentsCopy = components.Values.ToList();

            isIteratingComponents = true;
            try
            {
                foreach (var component in componentsCopy)
                {
                    if (component is BehaviorComponentBase behaviorComponent && behaviorComponent.IsEnabled)
                        behaviorComponent.OnDestroy();
                    component.OnDetach();
                }
            }
            finally
            {
                isIteratingComponents = false;
            }

            components.Clear();
            pendingComponents.Clear();
            pendingRemovals.Clear();
        }

        protected virtual void OnCollisionEnter(Collision collision)
        {
            var componentsCopy = components.Values.ToList();

            isIteratingComponents = true;
            try
            {
                foreach (var component in componentsCopy)
                    if (component is BehaviorComponentBase behaviorComponent && behaviorComponent.IsEnabled)
                        behaviorComponent.OnCollisionEnter(collision);
            }
            finally
            {
                isIteratingComponents = false;
                ProcessPendingOperations();
            }
        }

        protected virtual void OnCollisionStay(Collision collision)
        {
            var componentsCopy = components.Values.ToList();

            isIteratingComponents = true;
            try
            {
                foreach (var component in componentsCopy)
                    if (component is BehaviorComponentBase behaviorComponent && behaviorComponent.IsEnabled)
                        behaviorComponent.OnCollisionStay(collision);
            }
            finally
            {
                isIteratingComponents = false;
                ProcessPendingOperations();
            }
        }

        protected virtual void OnCollisionExit(Collision collision)
        {
            var componentsCopy = components.Values.ToList();

            isIteratingComponents = true;
            try
            {
                foreach (var component in componentsCopy)
                    if (component is BehaviorComponentBase behaviorComponent && behaviorComponent.IsEnabled)
                        behaviorComponent.OnCollisionExit(collision);
            }
            finally
            {
                isIteratingComponents = false;
                ProcessPendingOperations();
            }
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            var componentsCopy = components.Values.ToList();

            isIteratingComponents = true;
            try
            {
                foreach (var component in componentsCopy)
                    if (component is BehaviorComponentBase behaviorComponent && behaviorComponent.IsEnabled)
                        behaviorComponent.OnTriggerEnter(other);
            }
            finally
            {
                isIteratingComponents = false;
                ProcessPendingOperations();
            }
        }

        protected virtual void OnTriggerStay(Collider other)
        {
            var componentsCopy = components.Values.ToList();

            isIteratingComponents = true;
            try
            {
                foreach (var component in componentsCopy)
                    if (component is BehaviorComponentBase behaviorComponent && behaviorComponent.IsEnabled)
                        behaviorComponent.OnTriggerStay(other);
            }
            finally
            {
                isIteratingComponents = false;
                ProcessPendingOperations();
            }
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            var componentsCopy = components.Values.ToList();

            isIteratingComponents = true;
            try
            {
                foreach (var component in componentsCopy)
                    if (component is BehaviorComponentBase behaviorComponent && behaviorComponent.IsEnabled)
                        behaviorComponent.OnTriggerExit(other);
            }
            finally
            {
                isIteratingComponents = false;
                ProcessPendingOperations();
            }
        }

        protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
            var componentsCopy = components.Values.ToList();

            isIteratingComponents = true;
            try
            {
                foreach (var component in componentsCopy)
                    if (component is BehaviorComponentBase behaviorComponent && behaviorComponent.IsEnabled)
                        behaviorComponent.OnCollisionEnter2D(collision);
            }
            finally
            {
                isIteratingComponents = false;
                ProcessPendingOperations();
            }
        }

        protected virtual void OnCollisionStay2D(Collision2D collision)
        {
            var componentsCopy = components.Values.ToList();

            isIteratingComponents = true;
            try
            {
                foreach (var component in componentsCopy)
                    if (component is BehaviorComponentBase behaviorComponent && behaviorComponent.IsEnabled)
                        behaviorComponent.OnCollisionStay2D(collision);
            }
            finally
            {
                isIteratingComponents = false;
                ProcessPendingOperations();
            }
        }

        protected virtual void OnCollisionExit2D(Collision2D collision)
        {
            var componentsCopy = components.Values.ToList();

            isIteratingComponents = true;
            try
            {
                foreach (var component in componentsCopy)
                    if (component is BehaviorComponentBase behaviorComponent && behaviorComponent.IsEnabled)
                        behaviorComponent.OnCollisionExit2D(collision);
            }
            finally
            {
                isIteratingComponents = false;
                ProcessPendingOperations();
            }
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            var componentsCopy = components.Values.ToList();

            isIteratingComponents = true;
            try
            {
                foreach (var component in componentsCopy)
                    if (component is BehaviorComponentBase behaviorComponent && behaviorComponent.IsEnabled)
                        behaviorComponent.OnTriggerEnter2D(other);
            }
            finally
            {
                isIteratingComponents = false;
                ProcessPendingOperations();
            }
        }

        protected virtual void OnTriggerStay2D(Collider2D other)
        {
            var componentsCopy = components.Values.ToList();

            isIteratingComponents = true;
            try
            {
                foreach (var component in componentsCopy)
                    if (component is BehaviorComponentBase behaviorComponent && behaviorComponent.IsEnabled)
                        behaviorComponent.OnTriggerStay2D(other);
            }
            finally
            {
                isIteratingComponents = false;
                ProcessPendingOperations();
            }
        }

        protected virtual void OnTriggerExit2D(Collider2D other)
        {
            var componentsCopy = components.Values.ToList();

            isIteratingComponents = true;
            try
            {
                foreach (var component in componentsCopy)
                    if (component is BehaviorComponentBase behaviorComponent && behaviorComponent.IsEnabled)
                        behaviorComponent.OnTriggerExit2D(other);
            }
            finally
            {
                isIteratingComponents = false;
                ProcessPendingOperations();
            }
        }

        protected virtual void OnDrawGizmos()
        {
            var componentsCopy = components.Values.ToList();

            isIteratingComponents = true;
            try
            {
                foreach (var component in componentsCopy)
                    if (component is BehaviorComponentBase behaviorComponent && behaviorComponent.IsEnabled)
                        behaviorComponent.OnDrawGizmos();
            }
            finally
            {
                isIteratingComponents = false;
                ProcessPendingOperations();
            }
        }

        #endregion
    }
}