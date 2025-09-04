using HappyHotel.Action.Components.Parts;
using HappyHotel.Core.EntityComponent;
using UnityEngine;

namespace HappyHotel.Action
{
    // 基于护甲值的攻击行动，造成等于当前护甲值的伤害
    [AutoInitEntityComponent(typeof(AttackEntityComponent))]
    [AutoInitEntityComponent(typeof(ArmorValueTrackerComponent))]
    public class ArmorBasedAttackAction : ActionBase
    {
        private readonly AttackEntityComponent attackComponent;
        private readonly ArmorValueTrackerComponent trackerComponent;

        public ArmorBasedAttackAction()
        {
            attackComponent = GetEntityComponent<AttackEntityComponent>();
            trackerComponent = GetEntityComponent<ArmorValueTrackerComponent>();

            UpdateDamage();

            // 订阅护甲值改变事件
            if (trackerComponent != null) trackerComponent.onArmorValueChanged += ArmorValueChanged;

            // 订阅攻击处理器变化，确保最终值变化时通知UI
            var attackValue = attackComponent?.GetAttackValue();
            if (attackValue != null) attackValue.onProcessorsChanged.AddListener(OnProcessorsChanged);
        }

        // 重写GetActionValue方法，返回当前的攻击伤害
        public override int GetActionValue()
        {
            return attackComponent?.Damage ?? 0;
        }

        ~ArmorBasedAttackAction()
        {
            if (trackerComponent != null) trackerComponent.onArmorValueChanged -= ArmorValueChanged;
            var attackValue = attackComponent?.GetAttackValue();
            if (attackValue != null) attackValue.onProcessorsChanged.RemoveListener(OnProcessorsChanged);
        }

        // 处理护甲值改变事件
        private void ArmorValueChanged(int newArmorValue)
        {
            UpdateDamage();
        }

        // 根据当前护甲值更新伤害
        private void UpdateDamage()
        {
            if (attackComponent == null || trackerComponent == null)
                return;

            // 设置伤害等于当前护甲值
            var armorBasedDamage = GetArmorBasedDamage();
            attackComponent.SetDamage(armorBasedDamage);

            // 通知数值变化
            NotifyActionValueChanged(armorBasedDamage);

            Debug.Log($"护甲攻击: 更新伤害为 {armorBasedDamage} (基于当前护甲值)");
        }

        private void OnProcessorsChanged()
        {
            NotifyActionValueChanged(GetActionValue());
        }

        // 获取当前基于护甲的伤害值
        public int GetArmorBasedDamage()
        {
            return trackerComponent?.CurrentArmorValue / 2 ?? 0;
        }

        // 占位符格式化：{damage} {currentArmor}
        protected override string FormatDescriptionInternal(string formattedDescription)
        {
            var dmg = GetActionValue();
            var armor = trackerComponent?.CurrentArmorValue ?? 0;
            return formattedDescription
                .Replace("{damage}", dmg.ToString())
                .Replace("{currentArmor}", armor.ToString());
        }
    }
}