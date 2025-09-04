using System;
using UnityEngine;
using UnityEngine.Events;

namespace HappyHotel.Core.ValueProcessing
{
    // 生命值实现
    [Serializable]
    public class HitPointValue : ProcessableValue
    {
        // 死亡事件
        public UnityEvent onDeath = new();

        public bool IsDead { get; private set; }

        protected override void OnValueDecreased(int amount, IComponentContainer source)
        {
            base.OnValueDecreased(amount, source);

            Debug.Log($"{owner?.Name} 受到 {amount} 点伤害，当前生命值: {currentValue}");

            if (currentValue <= 0 && !IsDead) Die();
        }

        protected override void OnValueIncreased(int amount, IComponentContainer source)
        {
            base.OnValueIncreased(amount, source);

            Debug.Log($"{owner?.Name} 恢复 {amount} 点生命值，当前生命值: {currentValue}");

            // 如果从死亡状态恢复
            if (IsDead && currentValue > 0) IsDead = false;
        }

        // 造成伤害
        public int TakeDamage(int damage, IComponentContainer attacker = null)
        {
            return DecreaseValue(damage, attacker);
        }

        // 新增：带来源类型的伤害
        public int TakeDamage(int damage, DamageSourceType sourceType, IComponentContainer attacker = null)
        {
            var context = ValueChangeContext.Create(sourceType, attacker);
            return DecreaseValue(damage, context, attacker);
        }

        // 治疗
        public int Heal(int amount, IComponentContainer healer = null)
        {
            return IncreaseValue(amount, healer);
        }

        // 新增：带上下文的治疗（占位，当前未使用来源区分）
        public int Heal(int amount, DamageSourceType sourceType, IComponentContainer healer = null)
        {
            var context = ValueChangeContext.Create(sourceType, healer);
            return IncreaseValue(amount, context, healer);
        }

        // 死亡处理
        private void Die()
        {
            IsDead = true;
            onDeath.Invoke();
            Debug.Log($"{owner?.Name} 死亡");
        }

        // 复活
        public void Revive(int hitPoints = -1)
        {
            if (hitPoints < 0)
                hitPoints = maxValue;

            IsDead = false;
            SetCurrentValue(hitPoints);
            Debug.Log($"{owner?.Name} 复活，生命值: {currentValue}");
        }

        public override void Dispose()
        {
            base.Dispose();
            onDeath.RemoveAllListeners();
        }
    }
}