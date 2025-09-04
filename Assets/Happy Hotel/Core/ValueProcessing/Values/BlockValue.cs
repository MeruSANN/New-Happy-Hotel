using System;
using HappyHotel.Core.ValueProcessing.Components;
using UnityEngine;

namespace HappyHotel.Core.ValueProcessing
{
    // 格挡值实现
    [Serializable]
    public class BlockValue : ProcessableValue
    {
        // 格挡值清除事件
        public event Action<int, BlockClearReason> onBlockValueCleared; // (oldValue, reason)

        // 格挡值消耗事件（因伤害减少）
        public event Action<int, int> onBlockValueConsumed; // (oldValue, newValue)

        protected override void OnValueIncreased(int amount, IComponentContainer source)
        {
            base.OnValueIncreased(amount, source);
            Debug.Log($"{owner?.Name} 获得 {amount} 点格挡，当前格挡值: {currentValue}");
        }

        protected override void OnValueDecreased(int amount, IComponentContainer source)
        {
            base.OnValueDecreased(amount, source);
            Debug.Log($"{owner?.Name} 失去 {amount} 点格挡，当前格挡值: {currentValue}");
        }

        // 添加格挡
        public int AddBlock(int amount, IComponentContainer source = null)
        {
            return IncreaseValue(amount, source);
        }

        // 消耗格挡并返回剩余伤害
        public int ConsumeBlock(int damage)
        {
            if (currentValue <= 0 || damage <= 0)
                return damage;

            var oldValue = currentValue;
            var blockUsed = Mathf.Min(currentValue, damage);
            DecreaseValue(blockUsed);

            // 触发消耗事件
            onBlockValueConsumed?.Invoke(oldValue, currentValue);

            return damage - blockUsed;
        }

        // 清除所有格挡（带原因参数）
        public void ClearBlock(BlockClearReason reason = BlockClearReason.Manual)
        {
            if (currentValue > 0)
            {
                var oldValue = currentValue;
                SetCurrentValue(0);

                // 触发清除事件
                onBlockValueCleared?.Invoke(oldValue, reason);

                Debug.Log($"{owner?.Name} 失去了所有格挡 (原因: {reason})");
            }
        }

        // 重写Dispose方法，清理事件
        public override void Dispose()
        {
            onBlockValueCleared = null;
            onBlockValueConsumed = null;
            base.Dispose();
        }
    }
}