using HappyHotel.Action.Components;
using HappyHotel.Action.Components.Parts;
using HappyHotel.Character;
using HappyHotel.Core.EntityComponent;
using UnityEngine;

namespace HappyHotel.Action
{
    // 先攻攻击行动+，如果是回合的第一个行动则造成基础伤害的双倍伤害
    [AutoInitEntityComponent(typeof(AttackEntityComponent))]
    public class FirstAttackPlusAction : ActionBase
    {
        private readonly AttackEntityComponent attackComponent;
        private TurnActionCounterComponent turnActionCounterComponent;

        public FirstAttackPlusAction()
        {
            attackComponent = GetEntityComponent<AttackEntityComponent>();
            UpdateDamage();

            var attackValue = attackComponent?.GetAttackValue();
            if (attackValue != null) attackValue.onProcessorsChanged.AddListener(OnProcessorsChanged);
        }

        public int BaseDamage { get; private set; } = 2;

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
                    Debug.Log("FirstAttackPlusAction: 成功连接到TurnActionCounterComponent");
                }
                else
                {
                    Debug.LogWarning("FirstAttackPlusAction: 未找到TurnActionCounterComponent");
                }
            }
        }

        // 处理行动计数改变事件
        private void OnActionCountChanged(int actionCount)
        {
            UpdateDamage();
        }

        // 根据是否为回合首个行动更新伤害
        private void UpdateDamage()
        {
            if (attackComponent == null)
                return;

            var totalDamage = BaseDamage;

            // 检查是否为回合的第一个行动
            if (turnActionCounterComponent != null)
            {
                var isFirstAction = turnActionCounterComponent.IsFirstActionOfTurn();
                totalDamage = isFirstAction ? BaseDamage * 2 : BaseDamage;

                Debug.Log($"FirstAttackPlusAction: 更新伤害 - 是否首个行动: {isFirstAction}, 伤害: {totalDamage}");
            }
            else
            {
                // 如果没有TurnActionCounterComponent，默认不给予额外伤害
                Debug.LogWarning("FirstAttackPlusAction: 没有TurnActionCounterComponent，使用基础伤害");
            }

            attackComponent.SetDamage(totalDamage);

            // 通知数值变化
            NotifyActionValueChanged(totalDamage);
        }

        private void OnProcessorsChanged()
        {
            NotifyActionValueChanged(GetActionValue());
        }

        // 设置基础伤害
        public void SetBaseDamage(int damage)
        {
            BaseDamage = Mathf.Max(0, damage);
            UpdateDamage();
        }

        // 获取基础伤害
        public int GetBaseDamage()
        {
            return BaseDamage;
        }

        // 检查当前是否会获得双倍伤害
        public bool WillGetDoubleDamage()
        {
            return turnActionCounterComponent?.IsFirstActionOfTurn() ?? false;
        }

        ~FirstAttackPlusAction()
        {
            // 解除事件监听
            if (turnActionCounterComponent != null)
                turnActionCounterComponent.onActionCountChanged -= OnActionCountChanged;
            var attackValue = attackComponent?.GetAttackValue();
            if (attackValue != null) attackValue.onProcessorsChanged.RemoveListener(OnProcessorsChanged);
        }

        // 占位符格式化：{baseDamage} {totalDamage} {double}
        protected override string FormatDescriptionInternal(string formattedDescription)
        {
            var total = GetActionValue();
            var doubled = WillGetDoubleDamage();
            return formattedDescription
                .Replace("{baseDamage}", BaseDamage.ToString())
                .Replace("{totalDamage}", total.ToString())
                .Replace("{double}", doubled ? "是" : "否");
        }
    }
}