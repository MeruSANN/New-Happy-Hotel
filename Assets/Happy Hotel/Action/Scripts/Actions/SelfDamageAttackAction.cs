using HappyHotel.Action.Components.Parts;
using HappyHotel.Core.EntityComponent;

namespace HappyHotel.Action
{
    // 自伤攻击行动，造成攻击伤害的同时对自己造成伤害
    [AutoInitEntityComponent(typeof(AttackEntityComponent))]
    [AutoInitEntityComponent(typeof(SelfDamageEntityComponent))]
    public class SelfDamageAttackAction : ActionBase
    {
        private readonly AttackEntityComponent attackComponent;
        private readonly SelfDamageEntityComponent selfDamageComponent;

        public SelfDamageAttackAction()
        {
            attackComponent = GetEntityComponent<AttackEntityComponent>();
            selfDamageComponent = GetEntityComponent<SelfDamageEntityComponent>();
            var attackValue = attackComponent?.GetAttackValue();
            if (attackValue != null) attackValue.onProcessorsChanged.AddListener(OnProcessorsChanged);
        }

        // 重写GetActionValues方法，返回双数值：[攻击伤害, 自伤伤害]
        public override int[] GetActionValues()
        {
            var currentAttackDamage = attackComponent?.Damage ?? 0;
            var currentSelfDamage = selfDamageComponent?.SelfDamage ?? 0;
            return new[] { currentAttackDamage, currentSelfDamage };
        }

        // 重写GetActionValue方法，返回主要数值（攻击伤害）
        public override int GetActionValue()
        {
            return attackComponent?.Damage ?? 0;
        }

        private void OnProcessorsChanged()
        {
            NotifyActionValueChanged(GetActionValue());
        }

        // 占位符格式化：{attackDamage} {selfDamage}
        protected override string FormatDescriptionInternal(string formattedDescription)
        {
            var attackDamage = attackComponent?.Damage ?? 0;
            var selfDamage = selfDamageComponent?.SelfDamage ?? 0;
            return formattedDescription
                .Replace("{attackDamage}", attackDamage.ToString())
                .Replace("{selfDamage}", selfDamage.ToString());
        }
    }
}