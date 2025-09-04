using System;
using HappyHotel.Action.Components;
using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.ValueProcessing.Processors;
using UnityEngine;

namespace HappyHotel.Core.ValueProcessing.Components
{
    // 格挡值清除原因枚举
    public enum BlockClearReason
    {
        Damage, // 因为受到伤害而减少
        AutoClear, // 因为行动执行前自动清除
        Manual // 手动清除
    }

    // 格挡值组件 - 使用新的数值处理系统
    [DependsOnComponent(typeof(HitPointValueComponent))]
    public class BlockValueComponent : BehaviorComponentBase
    {
        private int actionExecutionCount; // 总行动执行计数
        private ActionQueueComponent actionQueueComponent;
        private BlockValueProcessor blockProcessor;
        private int blockSuccessActionCount = -1; // 格挡成功时的行动计数

        private HitPointValueComponent hitPointComponent;

        // 最后阻挡的伤害值记录

        // 格挡成功状态跟踪
        private bool lastBlockWasSuccessful;

        public BlockValue BlockValue { get; private set; }

        public int CurrentBlock => BlockValue?.CurrentValue ?? 0;

        // 最后阻挡的伤害值
        public int LastBlockedDamage { get; private set; }

        // 格挡成功状态属性 - 带有效期检查
        public bool LastBlockWasSuccessful
        {
            get
            {
                // 检查并处理可能的状态过期
                CheckAndHandleStatusExpiry();
                return lastBlockWasSuccessful;
            }
        }

        // 格挡成功状态改变事件
        public event Action<bool> onBlockSuccessChanged;

        // 检查并处理格挡成功状态过期
        private void CheckAndHandleStatusExpiry()
        {
            if (!lastBlockWasSuccessful) return; // 如果本来就是false，不需要检查

            // 检查格挡成功状态是否仍然有效
            var isValid = actionExecutionCount <= blockSuccessActionCount + 1;

            // 如果已经过期，触发重置
            if (!isValid)
            {
                var originalCount = actionExecutionCount;
                ResetBlockSuccessState();
                Debug.Log($"{GetHost()?.name} 格挡成功状态过期，重置计数 (原计数: {originalCount})");
            }
        }

        // 统一的格挡成功状态重置方法
        private void ResetBlockSuccessState()
        {
            var wasSuccessful = lastBlockWasSuccessful;

            lastBlockWasSuccessful = false;
            blockSuccessActionCount = -1;
            actionExecutionCount = 0;
            LastBlockedDamage = 0;

            // 只在状态确实发生变化时才触发事件
            if (wasSuccessful)
            {
                onBlockSuccessChanged?.Invoke(false);
                Debug.Log($"{GetHost()?.name} 格挡成功状态重置为false");
            }
        }

        public override void OnAttach(BehaviorComponentContainer host)
        {
            base.OnAttach(host);

            // 获取生命值组件
            hitPointComponent = host.GetBehaviorComponent<HitPointValueComponent>();

            // 获取行动队列组件
            actionQueueComponent = host.GetBehaviorComponent<ActionQueueComponent>();

            // 只在格挡值未初始化时才创建新实例，避免重复初始化导致数值丢失
            if (BlockValue == null)
            {
                BlockValue = new BlockValue();
                BlockValue.Initialize(host);

                // 监听格挡值的内部变化事件
                BlockValue.onBlockValueCleared += OnBlockValueCleared;
                BlockValue.onBlockValueConsumed += OnBlockValueConsumed;
            }

            // 创建并注册格挡处理器到生命值
            if (hitPointComponent != null && blockProcessor == null)
            {
                blockProcessor = new BlockValueProcessor(BlockValue);
                hitPointComponent.RegisterProcessor(blockProcessor);
            }
            else if (hitPointComponent == null)
            {
                Debug.LogWarning($"{host.gameObject.name} 无法获取HitPointValueComponent，跳过格挡处理器注册");
            }

            // 监听行动队列事件
            if (actionQueueComponent != null)
            {
                // 避免重复监听
                actionQueueComponent.onActionQueueChanged -= OnActionQueueChanged;
                actionQueueComponent.onActionQueueChanged += OnActionQueueChanged;
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            // 注销处理器
            if (hitPointComponent != null && blockProcessor != null)
                hitPointComponent.UnregisterProcessor(blockProcessor);

            // 停止监听行动队列事件
            if (actionQueueComponent != null) actionQueueComponent.onActionQueueChanged -= OnActionQueueChanged;

            // 停止监听格挡值事件
            if (BlockValue != null)
            {
                BlockValue.onBlockValueCleared -= OnBlockValueCleared;
                BlockValue.onBlockValueConsumed -= OnBlockValueConsumed;
            }

            BlockValue?.Dispose();
        }

        // 添加格挡
        public int AddBlock(int amount, IComponentContainer source = null)
        {
            return BlockValue?.AddBlock(amount, source) ?? 0;
        }

        // 清除格挡（带原因参数）
        public void ClearBlock(BlockClearReason reason = BlockClearReason.Manual)
        {
            BlockValue?.ClearBlock(reason);
        }

        // 注册处理器到格挡值本身
        public void RegisterProcessor(IValueProcessor processor)
        {
            BlockValue?.RegisterProcessor(processor);
        }

        // 注销处理器
        public void UnregisterProcessor(IValueProcessor processor)
        {
            BlockValue?.UnregisterProcessor(processor);
        }

        // 处理行动队列事件
        private void OnActionQueueChanged(ActionQueueEventArgs args)
        {
            // 当行动即将执行时，根据开关决定是否清空格挡值
            if (args.EventType == ActionQueueEventType.BeforeActionExecution)
            {
                if (CurrentBlock > 0) ClearBlock(BlockClearReason.AutoClear);
            }
            // 当行动执行完毕时，增加行动计数并检查格挡成功状态
            else if (args.EventType == ActionQueueEventType.ActionConsumed)
            {
                actionExecutionCount++;

                // 主动检查格挡成功状态是否过期，确保事件能正确触发
                CheckAndHandleStatusExpiry();
            }
        }

        // 处理格挡值被清除事件
        private void OnBlockValueCleared(int oldValue, BlockClearReason reason)
        {
            // 根据清除原因更新格挡成功状态
            UpdateBlockSuccessStatus(reason, oldValue, 0);
        }

        // 处理格挡值被消耗事件（因伤害减少）
        private void OnBlockValueConsumed(int oldValue, int newValue)
        {
            // 记录本次阻挡的伤害值
            var blockedDamage = oldValue - newValue;
            LastBlockedDamage = blockedDamage;

            // 因伤害减少格挡值，说明格挡成功
            UpdateBlockSuccessStatus(BlockClearReason.Damage, oldValue, newValue);
        }

        // 根据格挡值变化原因更新格挡成功状态
        private void UpdateBlockSuccessStatus(BlockClearReason reason, int oldValue, int newValue)
        {
            // 根据清除原因判断格挡是否成功
            switch (reason)
            {
                case BlockClearReason.Damage:
                    // 因为受到伤害而减少格挡值，说明格挡成功阻挡了伤害
                    SetBlockSuccessStatus(true);
                    Debug.Log($"格挡成功: 格挡值从 {oldValue} 减少到 {newValue}");
                    break;

                case BlockClearReason.AutoClear:
                case BlockClearReason.Manual:
                    // 自动清除和手动清除不改变格挡成功状态
                    break;
            }
        }

        // 更新格挡成功状态
        private void SetBlockSuccessStatus(bool successful)
        {
            if (lastBlockWasSuccessful != successful)
            {
                if (successful)
                {
                    // 记录格挡成功时的行动计数
                    lastBlockWasSuccessful = true;
                    blockSuccessActionCount = actionExecutionCount;
                    onBlockSuccessChanged?.Invoke(true);
                    Debug.Log($"{GetHost()?.name} 格挡成功，记录行动计数: {blockSuccessActionCount}");
                }
                else
                {
                    // 使用统一的重置方法
                    ResetBlockSuccessState();
                }
            }
        }
    }
}