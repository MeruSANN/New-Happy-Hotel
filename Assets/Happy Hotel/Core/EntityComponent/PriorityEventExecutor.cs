using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HappyHotel.Core.EntityComponent
{
    // 优先级事件执行器，负责按优先级顺序执行组件事件
    public static class PriorityEventExecutor
    {
        // 缓存组件类型的优先级信息
        private static readonly Dictionary<Type, int> priorityCache = new();

        // 获取组件的执行优先级
        public static int GetComponentPriority(Type componentType)
        {
            if (priorityCache.TryGetValue(componentType, out var cachedPriority)) return cachedPriority;

            var priorityAttribute = componentType.GetCustomAttribute<ExecutionPriorityAttribute>();
            var priority = priorityAttribute?.Priority ?? 0;

            priorityCache[componentType] = priority;
            return priority;
        }

        // 按优先级执行事件监听器
        public static void ExecuteEventByPriority(IEnumerable<IEventListener> listeners, EntityComponentEvent evt)
        {
            // 按优先级分组
            var priorityGroups = listeners
                .Where(listener => listener.IsEnabled)
                .GroupBy(listener => GetComponentPriority(listener.GetType()))
                .OrderBy(group => group.Key); // 数值越小优先级越高

            // 按优先级顺序执行
            foreach (var group in priorityGroups)
            foreach (var listener in group)
                listener.OnEvent(evt);
        }
    }
}