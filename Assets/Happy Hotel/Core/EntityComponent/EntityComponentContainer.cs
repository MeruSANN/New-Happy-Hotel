using System;
using System.Collections.Generic;
using System.Linq;
using HappyHotel.Core.ValueProcessing.Components;
using UnityEngine;

namespace HappyHotel.Core.EntityComponent
{
    // 非Mono组件容器，管理对象上的所有组件
    [AutoInitEntityComponent(typeof(ProcessableValueCollectorComponent))]
    public class EntityComponentContainer : IDisposable, IComponentContainer
    {
        // 存储所有组件的字典
        private readonly Dictionary<Type, IEntityComponent> components = new();

        // 容器名称

        // Tag系统 - 直接存储标签
        private readonly HashSet<string> tags = new();

        // 是否已销毁

        // 构造函数 - 自动初始化
        public EntityComponentContainer(string name = null)
        {
            Name = name ?? "EntityContainer";
            Initialize();
        }

        #region 状态查询

        public bool IsDestroyed { get; private set; }

        #endregion

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
        public string Name { get; }

        // 获取GameObject（EntityContainer没有GameObject）
        public GameObject GetGameObject()
        {
            return null;
        }

        // 检查容器是否有效
        public bool IsValid => !IsDestroyed;

        #endregion

        #region 组件管理

        // 添加组件
        public IEntityComponent AddEntityComponent(IEntityComponent component)
        {
            if (component == null)
                throw new ArgumentNullException(nameof(component));

            if (IsDestroyed)
                throw new InvalidOperationException("无法向已销毁的容器添加组件");

            var type = component.GetType();

            // 如果组件已存在，先移除
            if (components.ContainsKey(type))
            {
                var existingComponent = components[type];
                existingComponent.OnDetach();
                components.Remove(type);
            }

            // 检查组件依赖
            var attributes = type.GetCustomAttributes(typeof(DependsOnEntityComponentAttribute), true);
            foreach (DependsOnEntityComponentAttribute attr in attributes)
                if (attr.AutoAdd && !HasEntityComponentOfType(attr.RequiredType))
                    try
                    {
                        // 创建依赖组件实例
                        var dependencyComponent = Activator.CreateInstance(attr.RequiredType) as IEntityComponent;
                        if (dependencyComponent != null) AddEntityComponent(dependencyComponent);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"创建依赖组件 {attr.RequiredType.Name} 失败: {e.Message}");
                        throw;
                    }

            // 添加组件
            components[type] = component;
            component.OnAttach(this);

            // 立即初始化新组件
            if (component is EntityComponentBase entityComponent && entityComponent.IsEnabled)
                entityComponent.OnInitialize();

            return component;
        }

        // 添加组件的泛型版本
        public T AddEntityComponent<T>() where T : class, IEntityComponent, new()
        {
            var component = new T();
            return AddEntityComponent(component) as T;
        }

        // 初始化通过Attribute标记的必需组件
        private void InitializeRequiredComponents()
        {
            // 保留原有的Attribute方式（向后兼容）
            var attributes = GetType().GetCustomAttributes(typeof(AutoInitEntityComponentAttribute), true);
            ProcessAutoInitAttributes(attributes, GetType().Name);

            // 新增：使用注册器初始化组件
            EntityComponentInitializerRegistry.InitializeComponents(this);
        }

        private void ProcessAutoInitAttributes(object[] attributes, string sourceName)
        {
            foreach (AutoInitEntityComponentAttribute attr in attributes)
                // 检查组件是否已存在
                if (!HasEntityComponentOfType(attr.ComponentType))
                    try
                    {
                        // 创建组件实例
                        var component = Activator.CreateInstance(attr.ComponentType) as IEntityComponent;

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
                            AddEntityComponent(component);
                            Debug.Log($"从 {sourceName} 自动初始化组件: {attr.ComponentType.Name}");
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"初始化组件 {attr.ComponentType.Name} 失败: {e.Message}\n{e.StackTrace}");
                    }
        }

        // 获取组件
        public T GetEntityComponent<T>() where T : class, IEntityComponent
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
        public bool RemoveEntityComponent<T>() where T : class, IEntityComponent
        {
            var type = typeof(T);

            if (components.TryGetValue(type, out var component))
            {
                // 检查是否有其他组件依赖于此组件
                foreach (var pair in components)
                {
                    var attributes = pair.Key.GetCustomAttributes(typeof(DependsOnEntityComponentAttribute), true);
                    foreach (DependsOnEntityComponentAttribute attr in attributes)
                        if (attr.RequiredType == type)
                        {
                            Debug.LogWarning($"无法移除组件 {type.Name}，因为组件 {pair.Key.Name} 依赖于它");
                            return false;
                        }
                }

                component.OnDetach();
                components.Remove(type);
                return true;
            }

            return false;
        }

        // 查询组件是否存在
        public bool HasEntityComponent<T>() where T : class, IEntityComponent
        {
            return GetEntityComponent<T>() != null;
        }

        // 检查是否存在指定类型的组件
        private bool HasEntityComponentOfType(Type type)
        {
            if (components.ContainsKey(type))
                return true;

            foreach (var pair in components)
                if (type.IsAssignableFrom(pair.Key))
                    return true;

            return false;
        }

        // 获取所有实现特定接口的组件
        public List<T> GetEntityComponents<T>() where T : class
        {
            return components.Values
                .OfType<T>()
                .ToList();
        }

        #endregion

        #region 事件系统

        // 发送事件到所有组件（按优先级执行）
        public void SendEvent(EntityComponentEvent evt)
        {
            var listeners = GetEntityComponents<IEventListener>();
            PriorityEventExecutor.ExecuteEventByPriority(listeners, evt);
        }

        // 发送事件到指定类型的组件（按优先级执行）
        public void SendEvent<T>(EntityComponentEvent evt) where T : class, IEventListener
        {
            var listeners = GetEntityComponents<T>();
            PriorityEventExecutor.ExecuteEventByPriority(listeners, evt);
        }

        #endregion

        #region 生命周期管理

        // 初始化容器
        private void Initialize()
        {
            if (IsDestroyed)
                return;

            // 初始化通过Attribute标记的必需组件
            InitializeRequiredComponents();

            // 初始化所有现有组件
            foreach (var component in components.Values)
                if (component is EntityComponentBase entityComponent && entityComponent.IsEnabled)
                    entityComponent.OnInitialize();
        }

        // 手动更新所有组件（可选调用）
        public void UpdateComponents()
        {
            if (IsDestroyed)
                return;

            foreach (var component in components.Values)
                if (component is EntityComponentBase entityComponent && entityComponent.IsEnabled)
                    entityComponent.OnUpdate();
        }

        // 销毁容器
        public void Dispose()
        {
            if (IsDestroyed)
                return;

            foreach (var component in components.Values)
            {
                if (component is EntityComponentBase entityComponent) entityComponent.OnDestroy();
                component.OnDetach();
            }

            components.Clear();
            tags.Clear();
            IsDestroyed = true;
        }

        #endregion
    }
}