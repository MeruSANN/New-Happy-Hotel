using System;
using UnityEngine;

namespace HappyHotel.Core.ValueProcessing
{
    // 装备数值实现 - 用于装备的各种数值属性
    [Serializable]
    public class EquipmentValue : ProcessableValue
    {
        public EquipmentValue(string valueName)
        {
            ValueName = valueName;
        }

        public string ValueName { get; private set; }

        // 移至基类：隐式转换操作符由 ProcessableValue 统一提供

        // 属性访问器，支持直接获取和设置数值
        public int Value
        {
            get => GetFinalValue();
            set => SetBaseValue(value);
        }

        protected override void OnValueIncreased(int amount, IComponentContainer source)
        {
            base.OnValueIncreased(amount, source);
            Debug.Log($"{owner?.Name} {ValueName}增加 {amount} 点，当前{ValueName}: {currentValue}");
        }

        protected override void OnValueDecreased(int amount, IComponentContainer source)
        {
            base.OnValueDecreased(amount, source);
            Debug.Log($"{owner?.Name} {ValueName}减少 {amount} 点，当前{ValueName}: {currentValue}");
        }

        // 设置基础数值（不触发处理器）
        public void SetBaseValue(int value)
        {
            SetCurrentValue(value);
        }

        // 获取基础数值（不经过处理器）
        public int GetRawValue()
        {
            return currentValue;
        }
    }
}