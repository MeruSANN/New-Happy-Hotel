using HappyHotel.Action.Components;
using HappyHotel.Action.Components.Parts;
using HappyHotel.Character;
using HappyHotel.Core.EntityComponent;
using UnityEngine;

namespace HappyHotel.Action
{
    // 后续攻击行动，当本回合执行过其他行动时造成额外伤害
    [AutoInitEntityComponent(typeof(AttackEntityComponent))]
    public class FollowUpAttackAction : ActionBase
    {
        private readonly AttackEntityComponent attackComponent;
        private TurnActionCounterComponent turnActionCounterComponent;

        public FollowUpAttackAction()
        {
            attackComponent = GetEntityComponent<AttackEntityComponent>();
            UpdateDamage();

            var attackValue = attackComponent?.GetAttackValue();
            if (attackValue != null) attackValue.onProcessorsChanged.AddListener(OnProcessorsChanged);
        }

        public int BaseDamage { get; private set; } = 2;

        public int BonusDamage { get; private set; } = 1;

        // 重写GetActionValue方法，返回当前的攻击伤害
        public override int GetActionValue()
        {
            return attackComponent?.Damage ?? 0;
        }

        public override void SetActionQueue(ActionQueueComponent actionQueue)
        {
            base.SetActionQueue(actionQueue);

            // 从ActionQueue的Host获取TurnActionCounterComponent
            if (actionQueue != null && actionQueue.GetHost() is CharacterBase character)
            {
                turnActionCounterComponent = character.GetBehaviorComponent<TurnActionCounterComponent>();
                if (turnActionCounterComponent != null)
                {
                    turnActionCounterComponent.onActionCountChanged += OnActionCountChanged;
                    UpdateDamage(); // 重新计算伤害
                    Debug.Log("FollowUpAttackAction: 成功连接到TurnActionCounterComponent");
                }
                else
                {
                    Debug.LogWarning("FollowUpAttackAction: 未找到TurnActionCounterComponent");
                }
            }
        }

        // 处理行动计数改变事件
        private void OnActionCountChanged(int actionCount)
        {
            UpdateDamage();
        }

        // 根据是否执行过其他行动更新伤害
        private void UpdateDamage()
        {
            if (attackComponent == null)
                return;

            var totalDamage = BaseDamage;

            // 检查是否执行过其他行动
            if (turnActionCounterComponent != null)
            {
                var hasExecutedOtherActions = turnActionCounterComponent.GetCurrentTurnActionCount() > 0;
                totalDamage = hasExecutedOtherActions ? BaseDamage + BonusDamage : BaseDamage;

                Debug.Log($"FollowUpAttackAction: 更新伤害 - 是否执行过其他行动: {hasExecutedOtherActions}, 伤害: {totalDamage}");
            }
            else
            {
                // 如果没有TurnActionCounterComponent，默认不给予额外伤害
                Debug.LogWarning("FollowUpAttackAction: 没有TurnActionCounterComponent，使用基础伤害");
            }

            attackComponent.SetDamage(totalDamage);

            // 通知数值变化
            NotifyActionValueChanged(totalDamage);
        }

        private void OnProcessorsChanged()
        {
            NotifyActionValueChanged(GetActionValue());
        }

        // 占位符格式化：{baseDamage} {bonusDamage} {totalDamage} {willBonus}
        protected override string FormatDescriptionInternal(string formattedDescription)
        {
            var totalDamage = GetActionValue();
            var willBonus = WillGetBonus();
            return formattedDescription
                .Replace("{baseDamage}", BaseDamage.ToString())
                .Replace("{bonusDamage}", BonusDamage.ToString())
                .Replace("{totalDamage}", totalDamage.ToString())
                .Replace("{willBonus}", willBonus ? "是" : "否");
        }

        // 设置基础伤害
        public void SetBaseDamage(int damage)
        {
            BaseDamage = Mathf.Max(0, damage);
            UpdateDamage();
        }

        // 设置额外伤害
        public void SetBonusDamage(int damage)
        {
            BonusDamage = Mathf.Max(0, damage);
            UpdateDamage();
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

        // 检查当前是否会获得额外伤害
        public bool WillGetBonus()
        {
            return turnActionCounterComponent?.GetCurrentTurnActionCount() > 0;
        }

        ~FollowUpAttackAction()
        {
            // 解除事件监听
            if (turnActionCounterComponent != null)
                turnActionCounterComponent.onActionCountChanged -= OnActionCountChanged;
            var attackValue = attackComponent?.GetAttackValue();
            if (attackValue != null) attackValue.onProcessorsChanged.RemoveListener(OnProcessorsChanged);
        }
    }
}