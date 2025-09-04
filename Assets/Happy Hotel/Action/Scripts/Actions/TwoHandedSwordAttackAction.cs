using HappyHotel.Action.Components;
using HappyHotel.Action.Components.Parts;
using HappyHotel.Character;
using HappyHotel.Core.EntityComponent;
using UnityEngine;

namespace HappyHotel.Action
{
    // 双手剑攻击行动，造成基础伤害，如果前一个行动是等待则造成额外伤害
    [AutoInitEntityComponent(typeof(AttackEntityComponent))]
    public class TwoHandedSwordAttackAction : ActionBase
    {
        private readonly AttackEntityComponent attackComponent;
        private LastConsumedActionTrackerComponent trackerComponent;

        public TwoHandedSwordAttackAction()
        {
            attackComponent = GetEntityComponent<AttackEntityComponent>();

            UpdateDamage();

            // 订阅处理器变化
            var attackValue = attackComponent?.GetAttackValue();
            if (attackValue != null) attackValue.onProcessorsChanged.AddListener(OnProcessorsChanged);
        }

        public int BaseDamage { get; private set; } = 1;

        public int BonusDamage { get; private set; } = 1;

        // 重写GetActionValue方法，返回当前的攻击伤害
        public override int GetActionValue()
        {
            return attackComponent?.Damage ?? 0;
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
                    UpdateDamage(); // 重新计算伤害
                }
            }
        }

        // 处理前一个行动改变事件
        private void LastConsumedActionChanged(IAction lastAction)
        {
            UpdateDamage();
        }

        // 根据前一个行动更新伤害
        private void UpdateDamage()
        {
            if (attackComponent == null)
                return;

            var totalDamage = BaseDamage;

            // 如果有 trackerComponent，检查前一个行动
            if (trackerComponent != null)
            {
                // 检查前一个消耗的行动是否为等待
                var waitTypeId = Core.Registry.TypeId.Create<ActionTypeId>("Wait");
                var wasLastActionWait = trackerComponent.IsLastConsumedActionOfTypeId(waitTypeId);

                // 根据前一个行动设置伤害
                totalDamage = wasLastActionWait ? BaseDamage + BonusDamage : BaseDamage;
            }

            attackComponent.SetDamage(totalDamage);

            // 通知数值变化
            NotifyActionValueChanged(totalDamage);

            Debug.Log(
                $"双手剑攻击: 更新伤害为 {totalDamage} (基础: {BaseDamage}, 额外: {(totalDamage > BaseDamage ? BonusDamage : 0)})");
        }

        private void OnProcessorsChanged()
        {
            // 处理器变化影响最终伤害，通知UI
            NotifyActionValueChanged(GetActionValue());
        }

        // 设置基础伤害
        public void SetBaseDamage(int damage)
        {
            BaseDamage = Mathf.Max(0, damage);
            UpdateDamage(); // 重新计算伤害
        }

        // 设置额外伤害
        public void SetBonusDamage(int damage)
        {
            BonusDamage = Mathf.Max(0, damage);
            UpdateDamage(); // 重新计算伤害
        }

        // 获取基础伤害
        public int GetBaseDamage()
        {
            return BaseDamage;
        }

        // 获取额外伤害
        public int GetBonusDamage()
        {
            return BonusDamage;
        }

        ~TwoHandedSwordAttackAction()
        {
            // 解除事件监听
            if (trackerComponent != null) trackerComponent.onLastConsumedActionChanged -= LastConsumedActionChanged;
            var attackValue = attackComponent?.GetAttackValue();
            if (attackValue != null) attackValue.onProcessorsChanged.RemoveListener(OnProcessorsChanged);
        }

        // 占位符格式化：{baseDamage} {bonusDamage} {totalDamage}
        protected override string FormatDescriptionInternal(string formattedDescription)
        {
            var total = GetActionValue();
            return formattedDescription
                .Replace("{baseDamage}", BaseDamage.ToString())
                .Replace("{bonusDamage}", BonusDamage.ToString())
                .Replace("{totalDamage}", total.ToString());
        }
    }
}