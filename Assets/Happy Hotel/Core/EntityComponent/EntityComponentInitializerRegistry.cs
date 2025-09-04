using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace HappyHotel.Core.EntityComponent
{
    // EntityComponent初始化器注册表
    public static class EntityComponentInitializerRegistry
    {
        private static readonly Dictionary<Type, List<IEntityComponentInitializer>> initializers = new();
        private static bool isInitialized;

        public static void Initialize()
        {
            if (isInitialized) return;

            // 扫描所有带有EntityComponentInitializerAttribute的类
            var initializerTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.GetCustomAttribute<EntityComponentInitializerAttribute>() != null)
                .Where(t => typeof(IEntityComponentInitializer).IsAssignableFrom(t));

            foreach (var initializerType in initializerTypes)
                try
                {
                    var attr = initializerType.GetCustomAttribute<EntityComponentInitializerAttribute>();
                    var initializer = (IEntityComponentInitializer)Activator.CreateInstance(initializerType);

                    if (!initializers.ContainsKey(attr.TargetContainerType))
                        initializers[attr.TargetContainerType] = new List<IEntityComponentInitializer>();

                    initializers[attr.TargetContainerType].Add(initializer);
                    Debug.Log($"注册EntityComponent初始化器: {initializerType.Name} -> {attr.TargetContainerType.Name}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"注册EntityComponent初始化器失败 {initializerType.Name}: {ex.Message}");
                }

            isInitialized = true;
        }

        public static void InitializeComponents(EntityComponentContainer container)
        {
            if (!isInitialized) Initialize();

            var containerType = container.GetType();

            // 初始化当前类型的组件
            if (initializers.TryGetValue(containerType, out var containerInitializers))
                foreach (var initializer in containerInitializers)
                    try
                    {
                        initializer.InitializeComponents(container);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"EntityComponent初始化失败: {ex.Message}");
                    }

            // 初始化基类的组件
            var baseType = containerType.BaseType;
            while (baseType != null && baseType != typeof(EntityComponentContainer))
            {
                if (initializers.TryGetValue(baseType, out var baseInitializers))
                    foreach (var initializer in baseInitializers)
                        try
                        {
                            initializer.InitializeComponents(container);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"基类EntityComponent初始化失败: {ex.Message}");
                        }

                baseType = baseType.BaseType;
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