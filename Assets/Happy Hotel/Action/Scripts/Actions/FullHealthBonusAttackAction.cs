using HappyHotel.Action.Components.Parts;
using HappyHotel.Core.EntityComponent;

namespace HappyHotel.Action
{
    // 满血额外伤害攻击行动，当敌人满血时造成额外伤害
    [AutoInitEntityComponent(typeof(AttackEntityComponent))]
    [AutoInitEntityComponent(typeof(FullHealthBonusEntityComponent))]
    public class FullHealthBonusAttackAction : ActionBase
    {
        private readonly AttackEntityComponent attackComponent;
        private readonly FullHealthBonusEntityComponent fullHealthBonusComponent;

        public FullHealthBonusAttackAction()
        {
            attackComponent = GetEntityComponent<AttackEntityComponent>();
            fullHealthBonusComponent = GetEntityComponent<FullHealthBonusEntityComponent>();
        }

        // 重写GetActionValue方法，返回当前的攻击伤害
        public override int GetActionValue()
        {
            return attackComponent?.Damage ?? 0;
        }

        // 设置基础伤害
        public void SetBaseDamage(int damage)
        {
            if (attackComponent != null)
            {
                attackComponent.SetDamage(damage);
                NotifyActionValueChanged(attackComponent.Damage);
            }
        }

        // 设置额外伤害
        public void SetBonusDamage(int damage)
        {
            if (fullHealthBonusComponent != null) fullHealthBonusComponent.SetBonusDamage(damage);
        }

        // 获取基础伤害
        public int GetBaseDamage()
        {
            return attackComponent?.Damage ?? 0;
        }

        // 获取额外伤害
        public int GetBonusDamage()
        {
            return fullHealthBonusComponent?.BonusDamage ?? 0;
        }

        // 占位符格式化：{baseDamage} {bonusDamage} {totalDamage}
        protected override string FormatDescriptionInternal(string formattedDescription)
        {
            var baseDmg = GetBaseDamage();
            var bonus = GetBonusDamage();
            var total = GetActionValue();
            return formattedDescription
                .Replace("{baseDamage}", baseDmg.ToString())
                .Replace("{bonusDamage}", bonus.ToString())
                .Replace("{totalDamage}", total.ToString());
        }
    }
}