using HappyHotel.Action.Components;
using HappyHotel.Action.Components.Parts;
using HappyHotel.Character;
using HappyHotel.Core.EntityComponent;
using UnityEngine;

namespace HappyHotel.Action
{
    // 双手大盾格挡行动，提供基础格挡值，如果前一个行动是等待则提供额外格挡值
    [AutoInitEntityComponent(typeof(BlockEntityComponent))]
    public class TwoHandedShieldBlockAction : ActionBase
    {
        private readonly BlockEntityComponent blockComponent;
        private LastConsumedActionTrackerComponent trackerComponent;

        public TwoHandedShieldBlockAction()
        {
            blockComponent = GetEntityComponent<BlockEntityComponent>();

            UpdateBlock();
        }

        public int BaseBlock { get; private set; } = 2;

        public int BonusBlock { get; private set; } = 2;

        // 重写GetActionValue方法，返回当前的格挡值
        public override int GetActionValue()
        {
            return blockComponent?.BlockAmount ?? 0;
        }

        public override void SetActionQueue(ActionQueueComponent actionQueue)
        {
            base.SetActionQueue(actionQueue);

            // 从 Character 获取 LastConsumedActionTrackerComponent
            if (actionQueue != null && actionQueue.GetHost() is CharacterBase character)
            {
                trackerComponent = character.GetBehaviorComponent<LastConsumedActionTrackerComponent>();
                if (trackerComponent != null)
                {
                    trackerComponent.onLastConsumedActionChanged += LastConsumedActionChanged;
                    UpdateBlock(); // 重新计算格挡值
                }
            }
        }

        // 处理前一个行动改变事件
        private void LastConsumedActionChanged(IAction lastAction)
        {
            UpdateBlock();
        }

        // 根据前一个行动更新格挡值
        private void UpdateBlock()
        {
            if (blockComponent == null)
                return;

            var totalBlock = BaseBlock;

            // 如果有 trackerComponent，检查前一个行动
            if (trackerComponent != null)
            {
                // 检查前一个消耗的行动是否为等待
                var waitTypeId = Core.Registry.TypeId.Create<ActionTypeId>("Wait");
                var wasLastActionWait = trackerComponent.IsLastConsumedActionOfTypeId(waitTypeId);

                // 根据前一个行动设置格挡值
                totalBlock = wasLastActionWait ? BaseBlock + BonusBlock : BaseBlock;
            }

            blockComponent.SetBlockAmount(totalBlock);

            // 通知数值变化
            NotifyActionValueChanged(totalBlock);

            Debug.Log(
                $"双手大盾格挡: 更新格挡值为 {totalBlock} (基础: {BaseBlock}, 额外: {(totalBlock > BaseBlock ? BonusBlock : 0)})");
        }

        // 设置基础格挡值
        public void SetBaseBlock(int block)
        {
            BaseBlock = Mathf.Max(0, block);
            UpdateBlock(); // 重新计算格挡值
        }

        // 设置额外格挡值
        public void SetBonusBlock(int block)
        {
            BonusBlock = Mathf.Max(0, block);
            UpdateBlock(); // 重新计算格挡值
        }

        // 获取基础格挡值
        public int GetBaseBlock()
        {
            return BaseBlock;
        }

        // 获取额外格挡值
        public int GetBonusBlock()
        {
            return BonusBlock;
        }

        ~TwoHandedShieldBlockAction()
        {
            // 解除事件监听
            if (trackerComponent != null) trackerComponent.onLastConsumedActionChanged -= LastConsumedActionChanged;
        }

        // 占位符格式化：{baseBlock} {bonusBlock} {totalBlock}
        protected override string FormatDescriptionInternal(string formattedDescription)
        {
            var total = GetActionValue();
            return formattedDescription
                .Replace("{baseBlock}", BaseBlock.ToString())
                .Replace("{bonusBlock}", BonusBlock.ToString())
                .Replace("{totalBlock}", total.ToString());
        }
    }
}