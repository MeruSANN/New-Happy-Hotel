using System;
using System.Collections.Generic;
using HappyHotel.Core.ValueProcessing.Processors;
using HappyHotel.Core.ValueProcessing.Modifiers;
using UnityEngine;
using UnityEngine.Events;

namespace HappyHotel.Core.ValueProcessing
{
    // 可处理数值基类
    [Serializable]
    public abstract class ProcessableValue
    {
        protected int maxValue;
        protected int currentValue;

        // 数值变化事件
        public UnityEvent<int> onValueChanged = new();
        public UnityEvent<int, int> onValueChangedWithDelta = new(); // (newValue, delta)

        public UnityEvent<int> onMaxValueChanged = new();
        public UnityEvent<int, int> onMaxValueChangedWithDelta = new(); // (newValue, delta)
        public UnityEvent onProcessorsChanged = new();
        private System.Action cachedProcessorsChangedHandler;
        protected IComponentContainer owner;

        protected ValueProcessorManager processorManager = new();
        protected StatModifierManager modifierManager = new();

        public int MaxValue => maxValue;
        public int CurrentValue => currentValue;

        // 初始化数值
        public virtual void Initialize(IComponentContainer owner, int maxValue = 999, int currentValue = 0)
        {
            this.maxValue = maxValue;
            this.currentValue = currentValue;
            this.owner = owner;
            if (cachedProcessorsChangedHandler == null)
                cachedProcessorsChangedHandler = () => { onProcessorsChanged.Invoke(); };
            processorManager.onProcessorsChanged += cachedProcessorsChangedHandler;
            modifierManager.onModifiersChanged += cachedProcessorsChangedHandler;
        }

        public virtual void SetMaxValue(int value)
        {
            var oldValue = maxValue;
            maxValue = value;
            if (currentValue > maxValue) SetCurrentValue(value);

            if (oldValue != maxValue)
            {
                onMaxValueChanged.Invoke(currentValue);
                onMaxValueChangedWithDelta.Invoke(currentValue, currentValue - oldValue);
            }
        }

        // 设置当前值（不触发处理器）
        public virtual void SetCurrentValue(int value)
        {
            var oldValue = currentValue;
            currentValue = Mathf.Clamp(value, 0, maxValue);

            if (oldValue != currentValue)
            {
                onValueChanged.Invoke(currentValue);
                onValueChangedWithDelta.Invoke(currentValue, currentValue - oldValue);
            }
        }

        // 增加数值（会触发处理器）
        public virtual int IncreaseValue(int amount, IComponentContainer source = null)
        {
            if (amount <= 0) return 0;

            var processedAmount = processorManager.ProcessValue(amount, ValueChangeType.Increase);
            var oldValue = currentValue;
            currentValue = Mathf.Min(maxValue, currentValue + processedAmount);
            var actualIncrease = currentValue - oldValue;

            if (actualIncrease > 0)
            {
                onValueChanged.Invoke(currentValue);
                onValueChangedWithDelta.Invoke(currentValue, actualIncrease);
                OnValueIncreased(actualIncrease, source);
            }

            return actualIncrease;
        }

        // 减少数值（会触发处理器）
        public virtual int DecreaseValue(int amount, IComponentContainer source = null)
        {
            if (amount <= 0) return 0;

            var processedAmount = processorManager.ProcessValue(amount, ValueChangeType.Decrease);
            var oldValue = currentValue;
            currentValue = Mathf.Max(0, currentValue - processedAmount);
            var actualDecrease = oldValue - currentValue;

            if (actualDecrease > 0)
            {
                onValueChanged.Invoke(currentValue);
                onValueChangedWithDelta.Invoke(currentValue, -actualDecrease);
                OnValueDecreased(actualDecrease, source);
            }

            return actualDecrease;
        }

        // 携带上下文的数值增加
        public virtual int IncreaseValue(int amount, ValueChangeContext context, IComponentContainer source = null)
        {
            if (amount <= 0) return 0;

            var processedAmount = processorManager.ProcessValue(amount, ValueChangeType.Increase, context);
            var oldValue = currentValue;
            currentValue = Mathf.Min(maxValue, currentValue + processedAmount);
            var actualIncrease = currentValue - oldValue;

            if (actualIncrease > 0)
            {
                onValueChanged.Invoke(currentValue);
                onValueChangedWithDelta.Invoke(currentValue, actualIncrease);
                OnValueIncreased(actualIncrease, source);
            }

            return actualIncrease;
        }

        // 携带上下文的数值减少
        public virtual int DecreaseValue(int amount, ValueChangeContext context, IComponentContainer source = null)
        {
            if (amount <= 0) return 0;

            var processedAmount = processorManager.ProcessValue(amount, ValueChangeType.Decrease, context);
            var oldValue = currentValue;
            currentValue = Mathf.Max(0, currentValue - processedAmount);
            var actualDecrease = oldValue - currentValue;

            if (actualDecrease > 0)
            {
                onValueChanged.Invoke(currentValue);
                onValueChangedWithDelta.Invoke(currentValue, -actualDecrease);
                OnValueDecreased(actualDecrease, source);
            }

            return actualDecrease;
        }

        // 注册处理器
        public void RegisterProcessor(IValueProcessor processor)
        {
            processorManager?.RegisterProcessor(processor);
        }

        // 注销处理器
        public void UnregisterProcessor(IValueProcessor processor)
        {
            processorManager?.UnregisterProcessor(processor);
        }

        // 对外暴露处理器管理器的必要方法，便于Buff在宿主上卸载某一类堆叠
        public ValueProcessorManager GetProcessorManager()
        {
            return processorManager;
        }

        // 注册可叠加处理器（provider: 效果提供者）
        public void RegisterStackableProcessor<T>(int amount, object provider) where T : IStackableProcessor, new()
        {
            processorManager?.RegisterStackableProcessor<T>(amount, provider);
        }

        // 注册无提供者的可叠加处理器
        public void RegisterStackableProcessorWithoutSource<T>(int amount) where T : IStackableProcessor, new()
        {
            processorManager?.RegisterStackableProcessorWithoutSource<T>(amount);
        }

        // 注销可叠加处理器的特定提供者
        public void UnregisterStackableProcessor<T>(object provider) where T : IStackableProcessor
        {
            processorManager?.UnregisterStackableProcessor<T>(provider);
        }

        // 获取处理器信息
        public List<(string Name, int Priority, ValueChangeType SupportedTypes, string Details)> GetProcessorInfo()
        {
            return processorManager?.GetProcessorInfo() ?? new List<(string, int, ValueChangeType, string)>();
        }

        // 注册可叠加修饰器
        public void RegisterStackableModifier<T>(int amount, object provider) where T : IStackableStatModifier, new()
        {
            modifierManager?.RegisterStackableModifier<T>(amount, provider);
        }

        // 注册无提供者的可叠加修饰器
        public void RegisterStackableModifierWithoutSource<T>(int amount) where T : IStackableStatModifier, new()
        {
            modifierManager?.RegisterStackableModifierWithoutSource<T>(amount);
        }

        // 注销可叠加修饰器
        public void UnregisterStackableModifier<T>(object provider) where T : IStackableStatModifier
        {
            modifierManager?.UnregisterStackableModifier<T>(provider);
        }

        // 检查是否包含指定类型的可叠加修饰器
        public bool HasStackableModifier<T>(object provider) where T : IStackableStatModifier
        {
            return modifierManager?.HasStackableModifier<T>(provider) ?? false;
        }

        // 子类可重写的方法
        protected virtual void OnValueIncreased(int amount, IComponentContainer source)
        {
        }

        protected virtual void OnValueDecreased(int amount, IComponentContainer source)
        {
        }

        // 检查是否包含指定处理器
        public bool HasProcessor(IValueProcessor processor)
        {
            return processorManager?.HasProcessor(processor) ?? false;
        }

        // 检查是否包含指定类型的可叠加处理器
        public bool HasStackableProcessor<T>(object provider) where T : IStackableProcessor
        {
            return processorManager?.HasStackableProcessor<T>(provider) ?? false;
        }

        // 获取最终数值（数值本身修饰管线）
        public virtual int GetFinalValue()
        {
            var finalValue = modifierManager?.ApplyModifiers(currentValue) ?? currentValue;
            return finalValue;
        }

        // 清理资源
        public virtual void Dispose()
        {
            if (processorManager != null && cachedProcessorsChangedHandler != null)
                processorManager.onProcessorsChanged -= cachedProcessorsChangedHandler;
            if (modifierManager != null && cachedProcessorsChangedHandler != null)
                modifierManager.onModifiersChanged -= cachedProcessorsChangedHandler;
            processorManager?.Clear();
            modifierManager?.Clear();
            onValueChanged.RemoveAllListeners();
            onValueChangedWithDelta.RemoveAllListeners();
            onProcessorsChanged.RemoveAllListeners();
        }

        // 重写ToString方法，返回数值信息
        public override string ToString()
        {
            // 返回最终数值（应用修饰器后的值），确保描述等字符串显示动态叠加后的结果
            return $"{GetFinalValue()}";
        }

        // 隐式转换为int，统一在基类提供，返回最终值
        public static implicit operator int(ProcessableValue value)
        {
            return value?.GetFinalValue() ?? 0;
        }
    }
}