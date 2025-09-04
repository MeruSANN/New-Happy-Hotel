using System;
using UnityEngine;

namespace HappyHotel.Core.ValueProcessing
{
    // 护甲值实现
    [Serializable]
    public class ArmorValue : ProcessableValue
    {
        protected override void OnValueIncreased(int amount, IComponentContainer source)
        {
            base.OnValueIncreased(amount, source);
            Debug.Log($"{owner?.Name} 获得 {amount} 点护甲，当前护甲值: {currentValue}");
        }

        protected override void OnValueDecreased(int amount, IComponentContainer source)
        {
            base.OnValueDecreased(amount, source);
            Debug.Log($"{owner?.Name} 失去 {amount} 点护甲，当前护甲值: {currentValue}");
        }

        // 添加护甲
        public int AddArmor(int amount, IComponentContainer source = null)
        {
            return IncreaseValue(amount, source);
        }

        // 消耗护甲并返回剩余伤害
        public int ConsumeArmor(int damage)
        {
            if (currentValue <= 0 || damage <= 0)
                return damage;

            var armorUsed = Mathf.Min(currentValue, damage);
            DecreaseValue(armorUsed);
            return damage - armorUsed;
        }

        // 清除所有护甲
        public void ClearArmor()
        {
            if (currentValue > 0)
            {
                SetCurrentValue(0);
                Debug.Log($"{owner?.Name} 失去了所有护甲");
            }
        }
    }
}