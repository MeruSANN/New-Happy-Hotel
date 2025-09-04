using System;
using System.Collections.Generic;
using System.Linq;
using HappyHotel.Core.ValueProcessing.Processors;
using UnityEngine;

namespace HappyHotel.Core.ValueProcessing
{
    // 数值处理器管理器，支持可叠加处理器
    public class ValueProcessorManager
    {
        private readonly Dictionary<Type, IStackableProcessor> stackableProcessors = new();
        private bool isDirty;
        private List<IValueProcessor> regularProcessors = new();
        public event System.Action onProcessorsChanged;

        private void MarkDirtyAndNotify()
        {
            isDirty = true;
            onProcessorsChanged?.Invoke();
        }

        // 注册常规处理器
        public void RegisterProcessor(IValueProcessor processor)
        {
            if (processor is IStackableProcessor)
            {
                Debug.LogWarning($"尝试将可叠加处理器 {processor.GetType().Name} 注册为常规处理器，请使用 RegisterStackableProcessor 方法");
                return;
            }

            if (!regularProcessors.Contains(processor))
            {
                regularProcessors.Add(processor);
                if (processor is IProcessorManagerAware aware) aware.BindManager(this);
                MarkDirtyAndNotify();
                Debug.Log($"注册常规数值处理器: {processor.GetType().Name}, 优先级: {processor.Priority}");
            }
        }

        // 注册可叠加处理器
        public void RegisterStackableProcessor<T>(int amount, object provider) where T : IStackableProcessor, new()
        {
            var type = typeof(T);

            if (!stackableProcessors.ContainsKey(type))
            {
                stackableProcessors[type] = new T();
                if (stackableProcessors[type] is IProcessorManagerAware aware) aware.BindManager(this);
                MarkDirtyAndNotify();
                Debug.Log($"创建新的可叠加处理器: {type.Name}");
            }

            stackableProcessors[type].AddStack(amount, provider);
            onProcessorsChanged?.Invoke();
        }

        // 注册无来源可叠加处理器（用于即时性道具等不需要来源追踪的场景）
        public void RegisterStackableProcessorWithoutSource<T>(int amount) where T : IStackableProcessor, new()
        {
            var type = typeof(T);

            if (!stackableProcessors.ContainsKey(type))
            {
                stackableProcessors[type] = new T();
                if (stackableProcessors[type] is IProcessorManagerAware aware) aware.BindManager(this);
                MarkDirtyAndNotify();
                Debug.Log($"创建新的无来源可叠加处理器: {type.Name}");
            }

            // 使用null作为提供者，表示无来源
            stackableProcessors[type].AddStack(amount, null);
            onProcessorsChanged?.Invoke();
        }

        // 注销常规处理器
        public void UnregisterProcessor(IValueProcessor processor)
        {
            if (regularProcessors.Remove(processor))
            {
                if (processor is IProcessorManagerAware aware) aware.UnbindManager(this);
                MarkDirtyAndNotify();
                Debug.Log($"注销常规数值处理器: {processor.GetType().Name}");
            }
        }

        // 注销可叠加处理器的特定提供者
        public void UnregisterStackableProcessor<T>(object provider) where T : IStackableProcessor
        {
            var type = typeof(T);
            if (stackableProcessors.TryGetValue(type, out var processor))
            {
                processor.RemoveStack(provider);

                // 如果没有叠加效果了，移除整个处理器
                if (!processor.HasStacks())
                {
                    stackableProcessors.Remove(type);
                    if (processor is IProcessorManagerAware aware) aware.UnbindManager(this);
                    Debug.Log($"移除空的可叠加处理器: {type.Name}");
                }

                MarkDirtyAndNotify();
                onProcessorsChanged?.Invoke();
            }
        }

        // 新增：卸载某一类可叠加处理器的所有堆叠
        public void UnregisterAllStacksOf<T>() where T : IStackableProcessor
        {
            var type = typeof(T);
            if (stackableProcessors.TryGetValue(type, out var processor))
            {
                stackableProcessors.Remove(type);
                if (processor is IProcessorManagerAware aware) aware.UnbindManager(this);
                Debug.Log($"卸载所有堆叠并移除可叠加处理器: {type.Name}");
                MarkDirtyAndNotify();
                onProcessorsChanged?.Invoke();
            }
        }

        // 处理数值变化
        public int ProcessValue(int originalValue, ValueChangeType changeType)
        {
            if (isDirty) SortProcessors();

            var currentValue = originalValue;

            // 合并所有处理器并按优先级排序
            var allProcessors = regularProcessors
                .Concat(stackableProcessors.Values)
                .OrderBy(p => p.Priority)
                .ToList();

            foreach (var processor in allProcessors)
                // 检查处理器是否支持当前变化类型
                if ((processor.SupportedChangeTypes & changeType) != 0)
                {
                    var newValue = processor.ProcessValue(currentValue, changeType);
                    if (newValue != currentValue)
                    {
                        Debug.Log($"{processor.GetType().Name} 处理数值变化: {currentValue} -> {newValue} (类型: {changeType})");
                        currentValue = newValue;
                    }

                    if (changeType == ValueChangeType.Decrease && currentValue <= 0) break;
                }

            return currentValue;
        }

        // 支持上下文的处理流程
        public int ProcessValue(int originalValue, ValueChangeType changeType, ValueChangeContext context)
        {
            if (isDirty) SortProcessors();

            var currentValue = originalValue;

            var allProcessors = regularProcessors
                .Concat(stackableProcessors.Values)
                .OrderBy(p => p.Priority)
                .ToList();

            foreach (var processor in allProcessors)
                if ((processor.SupportedChangeTypes & changeType) != 0)
                {
                    int newValue;
                    if (processor is Processors.IContextualValueProcessor contextual)
                        newValue = contextual.ProcessValue(currentValue, changeType, context);
                    else
                        newValue = processor.ProcessValue(currentValue, changeType);

                    if (newValue != currentValue)
                    {
                        Debug.Log($"{processor.GetType().Name} 处理数值变化(含上下文): {currentValue} -> {newValue} (类型: {changeType}, 来源: {context.SourceType})");
                        currentValue = newValue;
                    }
                    if (changeType == ValueChangeType.Decrease && currentValue <= 0) break;
                }

            return currentValue;
        }

        // 按优先级排序处理器
        private void SortProcessors()
        {
            regularProcessors = regularProcessors.OrderBy(p => p.Priority).ToList();
            isDirty = false;
        }

        // 获取所有处理器信息（用于调试）
        public List<(string Name, int Priority, ValueChangeType SupportedTypes, string Details)> GetProcessorInfo()
        {
            if (isDirty) SortProcessors();

            var result = new List<(string, int, ValueChangeType, string)>();

            // 添加常规处理器信息
            foreach (var processor in regularProcessors)
                result.Add((processor.GetType().Name, processor.Priority, processor.SupportedChangeTypes, "常规处理器"));

            // 添加可叠加处理器信息
            foreach (var kvp in stackableProcessors)
            {
                var processor = kvp.Value;
                var details = $"可叠加处理器 (叠加数: {processor.GetStackCount()}, 总效果: {processor.GetTotalEffectValue()})";
                result.Add((processor.GetType().Name, processor.Priority, processor.SupportedChangeTypes, details));
            }

            return result.OrderBy(p => p.Item2).ToList();
        }

        // 检查是否包含指定处理器
        public bool HasProcessor(IValueProcessor processor)
        {
            return regularProcessors.Contains(processor);
        }

        // 检查是否包含指定类型的可叠加处理器
        public bool HasStackableProcessor<T>(object provider) where T : IStackableProcessor
        {
            var type = typeof(T);
            if (stackableProcessors.TryGetValue(type, out var processor)) return processor.HasStackFromProvider(provider);
            return false;
        }

        // 清理所有处理器
        public void Clear()
        {
            regularProcessors.Clear();
            stackableProcessors.Clear();
            isDirty = false;
            onProcessorsChanged?.Invoke();
        }
    }
}