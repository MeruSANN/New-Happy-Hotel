using System;
using UnityEngine;

namespace HappyHotel.Core.ValueProcessing
{
    // 攻击伤害值实现
    [Serializable]
    public class AttackValue : ProcessableValue
    {
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

        // 增加攻击伤害
        public int AddDamage(int amount, IComponentContainer source = null)
        {
            return IncreaseValue(amount, source);
        }

        // 减少攻击伤害
        public int ReduceDamage(int amount, IComponentContainer source = null)
        {
            return DecreaseValue(amount, source);
        }

        // 设置基础伤害（不触发处理器）
        public void SetBaseDamage(int damage)
        {
            SetCurrentValue(damage);
        }

        // 获取最终伤害（走数值修饰管线）
        public int GetFinalDamage()
        {
            return GetFinalValue();
        }
    }
}