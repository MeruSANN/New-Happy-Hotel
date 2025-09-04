using System.Collections.Generic;
using HappyHotel.Action.Components.Parts;

namespace HappyHotel.Action.Settings
{
    // 自伤攻击行动设置类
    public class SelfDamageAttackActionSetting : IActionSetting
    {
        private readonly int attackDamage;
        private readonly int selfDamage;
        private readonly HashSet<string> targetTags;

        public SelfDamageAttackActionSetting(int attackDamage = 2, int selfDamage = 1, params string[] tags)
        {
            this.attackDamage = attackDamage;
            this.selfDamage = selfDamage;
            targetTags = new HashSet<string>(tags ?? new string[0]);
        }

        public void ConfigureAction(ActionBase action)
        {
            if (action is SelfDamageAttackAction selfDamageAttackAction)
            {
                // 配置攻击组件
                var attackComponent = selfDamageAttackAction.GetEntityComponent<AttackEntityComponent>();
                if (attackComponent != null)
                {
                    attackComponent.SetDamage(attackDamage);
                    attackComponent.ClearTargetTags();
                    foreach (var tag in targetTags) attackComponent.AddTargetTag(tag);
                }

                // 配置护甲组件
                var armorComponent = selfDamageAttackAction.GetEntityComponent<SelfDamageEntityComponent>();
                if (armorComponent != null) armorComponent.SetSelfDamage(selfDamage);
            }
        }
    }
}