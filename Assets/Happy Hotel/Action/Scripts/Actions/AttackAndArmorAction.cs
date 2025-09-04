using HappyHotel.Action.Components.Parts;
using HappyHotel.Core.EntityComponent;

namespace HappyHotel.Action
{
    // 攻击并增加护甲的行动，同时执行攻击和护甲逻辑
    [AutoInitEntityComponent(typeof(AttackEntityComponent))]
    [AutoInitEntityComponent(typeof(ArmorEntityComponent))]
    public class AttackAndArmorAction : ActionBase
    {
        private AttackEntityComponent attackComponent;
        private bool subscribed;

        public AttackAndArmorAction()
        {
            attackComponent = GetEntityComponent<AttackEntityComponent>();
            TrySubscribeProcessorsChanged();
        }

        // 重写GetActionValue方法，返回当前的攻击伤害（主要数值）
        public override int GetActionValue()
        {
            var attackComponent = GetEntityComponent<AttackEntityComponent>();
            return attackComponent?.Damage ?? 0;
        }

        // 重写GetActionValues方法，返回攻击值和护甲值
        public override int[] GetActionValues()
        {
            attackComponent ??= GetEntityComponent<AttackEntityComponent>();
            var armorComponent = GetEntityComponent<ArmorEntityComponent>();

            var attackValue = attackComponent?.Damage ?? 0;
            var armorValue = armorComponent?.ArmorAmount ?? 0;

            return new[] { attackValue, armorValue };
        }

        private void TrySubscribeProcessorsChanged()
        {
            if (subscribed) return;
            if (attackComponent == null) return;
            var attackValue = attackComponent.GetAttackValue();
            if (attackValue == null) return;
            attackValue.onProcessorsChanged.AddListener(OnProcessorsChanged);
            subscribed = true;
        }

        private void OnProcessorsChanged()
        {
            NotifyActionValueChanged(GetActionValue());
        }

        // 占位符格式化：{damage} {attackDamage} {armor}
        protected override string FormatDescriptionInternal(string formattedDescription)
        {
            var values = GetActionValues();
            var attack = values.Length > 0 ? values[0] : GetActionValue();
            var armorComponent = GetEntityComponent<ArmorEntityComponent>();
            var armor = armorComponent?.ArmorAmount ?? 0;
            return formattedDescription
                .Replace("{damage}", attack.ToString())
                .Replace("{attackDamage}", attack.ToString())
                .Replace("{armor}", armor.ToString());
        }
    }
}