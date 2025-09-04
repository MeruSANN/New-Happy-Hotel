using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace HappyHotel.Core.BehaviorComponent
{
    // 组件初始化器注册表
    public static class ComponentInitializerRegistry
    {
        private static readonly Dictionary<Type, List<IComponentInitializer>> initializers = new();
        private static bool isInitialized;

        public static void Initialize()
        {
            if (isInitialized) return;

            // 扫描所有带有ComponentInitializerAttribute的类
            var initializerTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.GetCustomAttribute<ComponentInitializerAttribute>() != null)
                .Where(t => typeof(IComponentInitializer).IsAssignableFrom(t));

            foreach (var initializerType in initializerTypes)
                try
                {
                    var attr = initializerType.GetCustomAttribute<ComponentInitializerAttribute>();
                    var initializer = (IComponentInitializer)Activator.CreateInstance(initializerType);

                    if (!initializers.ContainsKey(attr.TargetContainerType))
                        initializers[attr.TargetContainerType] = new List<IComponentInitializer>();

                    initializers[attr.TargetContainerType].Add(initializer);
                    Debug.Log($"注册组件初始化器: {initializerType.Name} -> {attr.TargetContainerType.Name}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"注册组件初始化器失败 {initializerType.Name}: {ex.Message}");
                }

            isInitialized = true;
        }

        public static void InitializeComponents(BehaviorComponentContainer container)
        {
            if (!isInitialized) Initialize();

            var containerType = container.GetType();

            // 初始化当前类型的组件
            if (initializers.TryGetValue(containerType, out var containerInitializers))
                foreach (var initializer in containerInitializers)
                    try
                    {
                        // 先自动创建属性中声明的组件
                        AutoCreateDeclaredComponents(container, initializer);
                        // 然后调用初始化器进行配置
                        initializer.InitializeComponents(container);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"组件初始化失败: {ex.Message}");
                    }

            // 初始化基类的组件
            var baseType = containerType.BaseType;
            while (baseType != null && baseType != typeof(BehaviorComponentContainer))
            {
                if (initializers.TryGetValue(baseType, out var baseInitializers))
                    foreach (var initializer in baseInitializers)
                        try
                        {
                            AutoCreateDeclaredComponents(container, initializer);
                            initializer.InitializeComponents(container);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"基类组件初始化失败: {ex.Message}");
                        }

                baseType = baseType.BaseType;
            }
        }

        // 自动创建属性中声明的组件
        private static void AutoCreateDeclaredComponents(BehaviorComponentContainer container,
            IComponentInitializer initializer)
        {
            var initializerType = initializer.GetType();
            var attr = initializerType.GetCustomAttribute<ComponentInitializerAttribute>();

            if (attr?.ComponentTypes != null)
                foreach (var componentType in attr.ComponentTypes)
                    // 检查组件是否已存在
                    if (!container.HasBehaviorComponentOfType(componentType))
                        try
                        {
                            // 创建组件实例
                            var component = Activator.CreateInstance(componentType) as IBehaviorComponent;
                            if (component != null)
                            {
                                container.AddBehaviorComponent(component);
                                Debug.Log($"自动创建组件: {componentType.Name}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"自动创建组件失败 {componentType.Name}: {ex.Message}");
                        }
        }

        // 清理注册表（主要用于测试）
        public static void Clear()
        {
            initializers.Clear();
            isInitialized = false;
        }

        // 获取已注册的初始化器数量（用于调试）
        public static int GetInitializerCount()
        {
            return initializers.Values.Sum(list => list.Count);
        }
    }
}