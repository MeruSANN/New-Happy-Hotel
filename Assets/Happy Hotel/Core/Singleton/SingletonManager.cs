using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HappyHotel.Core.Singleton
{
    /// <summary>
    ///     单例管理器 - 负责管理所有标记了ManagedSingleton特性的单例
    ///     支持场景特定的单例加载/卸载，依赖关系管理，以及自动初始化
    /// </summary>
    public class SingletonManager : MonoBehaviour
    {
        public enum SingletonState
        {
            NotInitialized,
            Initializing,
            Initialized,
            Failed
        }

        // 依赖图
        private readonly Dictionary<Type, DependencyNode> dependencyGraph = new();

        // 初始化顺序
        private readonly List<Type> initializationOrder = new();

        // 存储单例的初始化状态
        private readonly Dictionary<Type, SingletonState> initializationStates = new();

        // 存储所有已创建的单例对象引用
        private readonly Dictionary<Type, MonoBehaviour> singletons = new();

        // 缓存所有单例类型信息
        private readonly Dictionary<Type, ManagedSingletonAttribute> singletonTypeCache = new();
        public static SingletonManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                // 缓存所有单例类型信息
                CacheSingletonTypes();

                // 初始化全局单例和当前场景单例
                InitializeGlobalSingletons();
                InitializeCurrentSceneSingletons();

                // 监听场景事件
                SceneManager.sceneLoaded += OnSceneLoaded;
                SceneManager.sceneUnloaded += OnSceneUnloaded;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                SceneManager.sceneUnloaded -= OnSceneUnloaded;
                Instance = null;
            }
        }

        // 缓存所有单例类型信息
        private void CacheSingletonTypes()
        {
            singletonTypeCache.Clear();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var singletonTypes = assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract &&
                            t.IsSubclassOf(typeof(MonoBehaviour)) &&
                            Attribute.IsDefined(t, typeof(ManagedSingletonAttribute)));

            foreach (var type in singletonTypes)
            {
                var attribute = type.GetCustomAttribute<ManagedSingletonAttribute>();
                singletonTypeCache[type] = attribute;
            }

            Debug.Log($"缓存了 {singletonTypeCache.Count} 个单例类型信息");
        }

        // 初始化全局单例（DontDestroyOnLoad的单例）
        public void InitializeGlobalSingletons()
        {
            try
            {
                var globalTypes = GetGlobalSingletonTypes();
                InitializeSingletonTypes(globalTypes, "全局单例");
            }
            catch (Exception e)
            {
                Debug.LogError($"初始化全局单例时出错: {e.Message}");
                LogSingletonException(e);
            }
        }

        // 初始化当前场景的单例
        public void InitializeCurrentSceneSingletons()
        {
            var currentScene = SceneManager.GetActiveScene();
            InitializeSceneSpecificSingletons(currentScene.name);
        }

        // 初始化指定场景的单例
        public void InitializeSceneSpecificSingletons(string sceneName)
        {
            try
            {
                var sceneTypes = GetSceneSingletonTypes(sceneName);
                InitializeSingletonTypes(sceneTypes, $"场景 {sceneName} 的单例");

                // 清理不应在当前场景存在的单例
                CleanupInvalidSingletons(sceneName);
            }
            catch (Exception e)
            {
                Debug.LogError($"初始化场景 {sceneName} 的单例时出错: {e.Message}");
                LogSingletonException(e);
            }
        }

        // 通用的单例类型初始化方法
        private void InitializeSingletonTypes(List<Type> types, string context)
        {
            if (types.Count == 0) return;

            // 构建依赖图
            BuildDependencyGraph(types);

            // 生成初始化顺序
            GenerateInitializationOrder();

            // 按顺序初始化单例
            foreach (var type in initializationOrder)
                if (!HasValidSingletonInstance(type))
                    CreateSingletonInstance(type);

            Debug.Log($"完成 {context} 初始化，共 {types.Count} 个单例");
        }

        // 获取全局单例类型
        private List<Type> GetGlobalSingletonTypes()
        {
            return singletonTypeCache
                .Where(kvp => kvp.Value.DontDestroyOnLoad)
                .Select(kvp => kvp.Key)
                .ToList();
        }

        // 获取场景特定单例类型
        private List<Type> GetSceneSingletonTypes(string sceneName)
        {
            return singletonTypeCache
                .Where(kvp => !kvp.Value.DontDestroyOnLoad && ShouldLoadInScene(kvp.Key, sceneName))
                .Select(kvp => kvp.Key)
                .ToList();
        }

        // 构建依赖图
        private void BuildDependencyGraph(List<Type> targetTypes)
        {
            dependencyGraph.Clear();

            // 创建依赖节点
            foreach (var type in targetTypes)
            {
                var node = new DependencyNode { SingletonType = type };
                dependencyGraph[type] = node;
            }

            // 添加依赖关系
            foreach (var type in targetTypes)
            {
                var node = dependencyGraph[type];
                var dependencies = type.GetCustomAttributes(typeof(SingletonInitializationDependencyAttribute), true)
                    .Cast<SingletonInitializationDependencyAttribute>();

                foreach (var dep in dependencies)
                {
                    // 检查依赖是否在目标类型中
                    if (!targetTypes.Contains(dep.DependencyType))
                    {
                        // 如果依赖不在目标类型中，检查是否已经存在实例
                        if (dep.IsRequired && !HasValidSingletonInstance(dep.DependencyType))
                            throw new SingletonInitializationException(
                                type,
                                SingletonInitializationException.InitializationErrorType.MissingDependency,
                                $"必需依赖 {dep.DependencyType.Name} 不可用");
                        continue;
                    }

                    node.Dependencies.Add(dep.DependencyType);
                    node.DependencyRequired[dep.DependencyType] = dep.IsRequired;
                    dependencyGraph[dep.DependencyType].DependedBy.Add(type);
                }
            }
        }

        // 使用Kahn算法生成拓扑排序的初始化顺序
        private void GenerateInitializationOrder()
        {
            initializationOrder.Clear();

            // 复制依赖图以进行处理
            var graph = new Dictionary<Type, HashSet<Type>>();
            foreach (var kvp in dependencyGraph) graph[kvp.Key] = new HashSet<Type>(kvp.Value.Dependencies);

            // 查找入度为0的节点（没有依赖的节点）
            var noDependencies = new Queue<Type>(
                graph.Where(kvp => kvp.Value.Count == 0)
                    .Select(kvp => kvp.Key));

            while (noDependencies.Count > 0)
            {
                var current = noDependencies.Dequeue();
                initializationOrder.Add(current);

                // 更新依赖于当前节点的其他节点
                if (dependencyGraph.ContainsKey(current))
                    foreach (var dependent in dependencyGraph[current].DependedBy)
                        if (graph.ContainsKey(dependent))
                        {
                            graph[dependent].Remove(current);
                            if (graph[dependent].Count == 0) noDependencies.Enqueue(dependent);
                        }
            }

            // 检查是否存在循环依赖
            if (initializationOrder.Count != dependencyGraph.Count)
            {
                var remainingTypes = graph.Where(kvp => kvp.Value.Count > 0)
                    .Select(kvp => kvp.Key);

                throw new SingletonInitializationException(
                    remainingTypes.First(),
                    SingletonInitializationException.InitializationErrorType.CircularDependency,
                    $"检测到循环依赖，涉及类型: {string.Join(", ", remainingTypes.Select(t => t.Name))}");
            }
        }

        // 检查特定类型的单例是否已经存在（保留原方法用于向后兼容）
        private bool HasSingletonInstance(Type type)
        {
            return HasValidSingletonInstance(type);
        }

        // 验证依赖关系
        private void ValidateDependencies(Type type)
        {
            if (!dependencyGraph.TryGetValue(type, out var node))
                return;

            foreach (var depType in node.Dependencies)
                if (node.DependencyRequired[depType] &&
                    (!initializationStates.ContainsKey(depType) ||
                     initializationStates[depType] != SingletonState.Initialized))
                    throw new SingletonInitializationException(
                        type,
                        SingletonInitializationException.InitializationErrorType.MissingDependency,
                        $"缺少必需的依赖 {depType.Name}");
        }

        // 创建指定类型的单例实例
        private void CreateSingletonInstance(Type type)
        {
            try
            {
                initializationStates[type] = SingletonState.Initializing;

                // 验证依赖
                ValidateDependencies(type);

                // 创建GameObject并添加组件
                var singletonObject = new GameObject(type.Name);
                var instance = singletonObject.AddComponent(type) as MonoBehaviour;

                if (instance != null)
                {
                    // 检查是否需要DontDestroyOnLoad
                    if (singletonTypeCache.TryGetValue(type, out var attribute) &&
                        attribute.DontDestroyOnLoad)
                        DontDestroyOnLoad(singletonObject);

                    singletons[type] = instance;
                    initializationStates[type] = SingletonState.Initialized;
                    Debug.Log($"已创建单例: {type.Name}");
                }
                else
                {
                    initializationStates[type] = SingletonState.Failed;
                    throw new SingletonInitializationException(
                        type,
                        SingletonInitializationException.InitializationErrorType.InitializationFailed);
                }
            }
            catch (Exception e)
            {
                initializationStates[type] = SingletonState.Failed;
                if (!(e is SingletonInitializationException))
                    throw new SingletonInitializationException(
                        type,
                        SingletonInitializationException.InitializationErrorType.InitializationFailed,
                        e.Message);
                throw;
            }
        }

        // 获取指定类型的单例实例
        public T GetSingleton<T>() where T : MonoBehaviour
        {
            var type = typeof(T);

            if (singletons.TryGetValue(type, out var instance))
                return instance as T;

            // 如果不在字典中，尝试在场景中查找
            var foundInstance = FindObjectOfType<T>();
            if (foundInstance != null)
            {
                singletons[type] = foundInstance;
                initializationStates[type] = SingletonState.Initialized;
                return foundInstance;
            }

            // 如果场景中也没有，则创建一个新的实例
            if (singletonTypeCache.ContainsKey(type))
            {
                CreateSingletonInstance(type);
                return singletons[type] as T;
            }

            return null;
        }

        // 判断单例是否应该在指定场景加载
        private bool ShouldLoadInScene(Type singletonType, string sceneName)
        {
            if (!singletonTypeCache.TryGetValue(singletonType, out var attribute))
            {
                Debug.LogWarning($"ShouldLoadInScene: 单例 {singletonType.Name} 没有找到缓存的属性信息");
                return false;
            }

            switch (attribute.LoadMode)
            {
                case SceneLoadMode.All:
                    return true;

                case SceneLoadMode.Include:
                    return attribute.IncludeScenes?.Contains(sceneName) ?? false;

                case SceneLoadMode.Exclude:
                    return !(attribute.ExcludeScenes?.Contains(sceneName) ?? false);

                default:
                    return true;
            }
        }

        // 场景加载事件处理
        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"场景加载: {scene.name}");

            // 延迟一帧处理场景切换的单例管理逻辑，确保场景完全加载
            StartCoroutine(HandleSceneTransitionDelayed(scene.name));
        }

        // 场景卸载事件处理
        private void OnSceneUnloaded(UnityEngine.SceneManagement.Scene scene)
        {
            Debug.Log($"场景卸载: {scene.name}");
            // 场景卸载时的清理已经在OnSceneLoaded中的HandleSceneTransition里统一处理
        }

        // 延迟处理场景切换（解决时机问题）
        private IEnumerator HandleSceneTransitionDelayed(string sceneName)
        {
            // 等待一帧，确保场景完全加载
            yield return null;

            // 处理场景切换的单例管理逻辑
            HandleSceneTransition(sceneName);
        }

        // 强制刷新当前场景的单例状态（供外部调用）
        public void RefreshCurrentSceneSingletons()
        {
            var currentScene = SceneManager.GetActiveScene();
            Debug.Log($"强制刷新场景 {currentScene.name} 的单例状态");
            HandleSceneTransition(currentScene.name);
        }

        // 处理场景切换的单例管理逻辑
        private void HandleSceneTransition(string newSceneName)
        {
            Debug.Log($"开始处理场景 {newSceneName} 的单例管理");

            // 1. 首先清理不应在新场景存在的单例
            CleanupInvalidSingletonsForScene(newSceneName);

            // 2. 然后加载应该在新场景存在但当前缺失的单例
            LoadMissingSingletonsForScene(newSceneName);

            Debug.Log($"完成场景 {newSceneName} 的单例管理");
        }

        // 清理不应在指定场景存在的单例
        private void CleanupInvalidSingletonsForScene(string sceneName)
        {
            var toRemove = new List<Type>();

            foreach (var kvp in singletons)
            {
                var type = kvp.Key;

                // 跳过全局单例（DontDestroyOnLoad）
                if (singletonTypeCache.TryGetValue(type, out var attribute) && attribute.DontDestroyOnLoad)
                    continue;

                // 检查单例是否应该在当前场景存在
                if (!ShouldLoadInScene(type, sceneName)) toRemove.Add(type);
            }

            foreach (var type in toRemove)
                if (singletons.TryGetValue(type, out var singleton))
                {
                    if (singleton != null)
                    {
                        Debug.Log($"场景切换时销毁单例GameObject: {type.Name}");
                        Destroy(singleton.gameObject);
                    }
                    else
                    {
                        Debug.Log($"场景切换时清理已销毁的单例引用: {type.Name}");
                    }

                    // 无论GameObject是否为null，都要从字典中移除
                    singletons.Remove(type);
                    initializationStates.Remove(type);
                }
        }

        // 加载应该在指定场景存在但当前缺失的单例
        private void LoadMissingSingletonsForScene(string sceneName)
        {
            var missingTypes = new List<Type>();

            foreach (var kvp in singletonTypeCache)
            {
                var type = kvp.Key;
                var attribute = kvp.Value;

                // 跳过全局单例（已经在初始化时加载）
                if (attribute.DontDestroyOnLoad)
                    continue;

                // 检查单例是否应该在当前场景存在但当前缺失
                if (ShouldLoadInScene(type, sceneName) && !HasValidSingletonInstance(type)) missingTypes.Add(type);
            }

            if (missingTypes.Count > 0)
            {
                Debug.Log($"场景 {sceneName} 需要加载 {missingTypes.Count} 个缺失的单例");
                InitializeSingletonTypes(missingTypes, $"场景 {sceneName} 的缺失单例");
            }
        }

        // 检查特定类型的单例是否已经存在且有效
        private bool HasValidSingletonInstance(Type type)
        {
            // 检查字典中是否有记录
            if (singletons.TryGetValue(type, out var cachedSingleton))
            {
                // 如果GameObject仍然存在，则有效
                if (cachedSingleton != null) return true;

                // GameObject已被销毁但字典中还有引用，清理它
                singletons.Remove(type);
                initializationStates.Remove(type);
            }

            // 在场景中搜索现有实例
            var existingInstances = FindObjectsOfType(type);
            if (existingInstances != null && existingInstances.Length > 0)
            {
                var validInstance = existingInstances[0] as MonoBehaviour;
                singletons[type] = validInstance;
                initializationStates[type] = SingletonState.Initialized;
                return true;
            }

            return false;
        }

        // 清理不应在当前场景存在的单例（保留原方法用于向后兼容）
        private void CleanupInvalidSingletons(string sceneName)
        {
            CleanupInvalidSingletonsForScene(sceneName);
        }

        // 清理所有单例
        public void ClearSingletons()
        {
            foreach (var singleton in singletons.Values)
                if (singleton != null)
                    Destroy(singleton.gameObject);
            singletons.Clear();
            initializationStates.Clear();
        }

        public void ClearSingletonsImmediate()
        {
            foreach (var singleton in singletons.Values)
                if (singleton != null)
                    DestroyImmediate(singleton.gameObject);
            singletons.Clear();
            initializationStates.Clear();
        }

        // 记录单例异常信息
        private void LogSingletonException(Exception e)
        {
            if (e is SingletonInitializationException sie)
                Debug.LogError($"单例初始化异常 - 类型: {sie.ProblemType?.Name}, 错误: {sie.ErrorType}");
        }

        // 获取单例状态信息（用于调试）
        public Dictionary<Type, SingletonState> GetSingletonStates()
        {
            return new Dictionary<Type, SingletonState>(initializationStates);
        }

        // 获取已创建的单例列表（用于调试）
        public Dictionary<Type, MonoBehaviour> GetCreatedSingletons()
        {
            return new Dictionary<Type, MonoBehaviour>(singletons);
        }

        // 调试方法：打印当前场景的单例状态
        public void DebugPrintSingletonStates(string sceneName = null)
        {
            if (string.IsNullOrEmpty(sceneName))
                sceneName = SceneManager.GetActiveScene().name;

            Debug.Log($"=== 场景 {sceneName} 的单例状态 ===");

            foreach (var kvp in singletonTypeCache)
            {
                var type = kvp.Key;
                var attribute = kvp.Value;
                var shouldLoad = ShouldLoadInScene(type, sceneName);
                var hasInstance = HasValidSingletonInstance(type);
                var state = initializationStates.TryGetValue(type, out var s) ? s : SingletonState.NotInitialized;

                var loadModeDesc = attribute.DontDestroyOnLoad ? "全局" : $"{attribute.LoadMode}";
                var statusDesc = shouldLoad ? hasInstance ? "✓已加载" : "✗缺失" : hasInstance ? "✗多余" : "○不需要";

                Debug.Log($"[{statusDesc}] {type.Name} - {loadModeDesc} - {state}");
            }
        }

        private class DependencyNode
        {
            public readonly HashSet<Type> DependedBy = new();
            public readonly HashSet<Type> Dependencies = new();
            public readonly Dictionary<Type, bool> DependencyRequired = new();
            public Type SingletonType;
        }
    }
}