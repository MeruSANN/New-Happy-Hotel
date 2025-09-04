using System;
using UnityEngine;

namespace HappyHotel.Core.ValueProcessing
{
    // 攻击装备数值实现 - 专门用于装备的攻击伤害数值属性
    [Serializable]
    public class AttackEquipmentValue : EquipmentValue
    {
        public AttackEquipmentValue(string valueName) : base(valueName)
        {
        }

        // 隐式转换操作符，支持从int直接赋值
        public static implicit operator AttackEquipmentValue(int value)
        {
            var attackEquipmentValue = new AttackEquipmentValue("攻击伤害");
            attackEquipmentValue.SetBaseValue(value);
            return attackEquipmentValue;
        }

        // 隐式转换操作符，支持转换为int
        public static implicit operator int(AttackEquipmentValue attackEquipmentValue)
        {
            return attackEquipmentValue?.GetFinalValue() ?? 0;
        }

        protected override void OnValueIncreased(int amount, IComponentContainer source)
        {
            base.OnValueIncreased(amount, source);
            Debug.Log($"{owner?.Name} 攻击伤害增加 {amount} 点，当前攻击伤害: {currentValue}");
        }

        protected override void OnValueDecreased(int amount, IComponentContainer source)
        {
            base.OnValueDecreased(amount, source);
            Debug.Log($"{owner?.Name} 攻击伤害减少 {amount} 点，当前攻击伤害: {currentValue}");
        }
    }
}