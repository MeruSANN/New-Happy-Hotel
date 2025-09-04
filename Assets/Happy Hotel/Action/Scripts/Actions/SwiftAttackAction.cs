using HappyHotel.Action.Components.Parts;
using HappyHotel.Core.EntityComponent;

namespace HappyHotel.Action
{
    // 迅捷攻击行动，当前面的行动执行时自动执行
    [AutoInitEntityComponent(typeof(AttackEntityComponent))]
    [AutoInitEntityComponent(typeof(PredecessorChainComponent))]
    public class SwiftAttackAction : ActionBase
    {
        private readonly AttackEntityComponent attackComponent;

        public SwiftAttackAction()
        {
            attackComponent = GetEntityComponent<AttackEntityComponent>();
            var attackValue = attackComponent?.GetAttackValue();
            if (attackValue != null) attackValue.onProcessorsChanged.AddListener(OnProcessorsChanged);
        }

        // 重写GetActionValue方法，返回当前的攻击伤害
        public override int GetActionValue()
        {
            return attackComponent?.Damage ?? 0;
        }

        // 设置基础伤害
        public void SetDamage(int damage)
        {
            if (attackComponent != null)
            {
                attackComponent.SetDamage(damage);
                NotifyActionValueChanged(attackComponent.Damage);
            }
        }

        // 获取当前伤害
        public int GetDamage()
        {
            return attackComponent?.Damage ?? 0;
        }

        private void OnProcessorsChanged()
        {
            NotifyActionValueChanged(GetActionValue());
        }

        // 占位符格式化：{damage}
        protected override string FormatDescriptionInternal(string formattedDescription)
        {
            var dmg = GetDamage();
            return formattedDescription.Replace("{damage}", dmg.ToString());
        }
    }
}