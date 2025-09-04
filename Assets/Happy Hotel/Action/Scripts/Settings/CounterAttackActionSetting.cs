using System.Collections.Generic;
using HappyHotel.Action.Components.Parts;

namespace HappyHotel.Action.Settings
{
    public class CounterAttackActionSetting : IActionSetting
    {
        private readonly int baseDamage;
        private readonly HashSet<string> targetTags;

        public CounterAttackActionSetting(int baseDamage, params string[] tags)
        {
            this.baseDamage = baseDamage;
            targetTags = new HashSet<string>(tags ?? new string[0]);
        }

        public void ConfigureAction(ActionBase action)
        {
            if (action is CounterAttackAction counterAttackAction)
            {
                // 设置基础伤害
                counterAttackAction.SetBaseDamage(baseDamage);

                // 配置攻击目标标签
                var attackComponent = counterAttackAction.GetEntityComponent<AttackEntityComponent>();
                if (attackComponent != null)
                {
                    attackComponent.ClearTargetTags();
                    foreach (var tag in targetTags) attackComponent.AddTargetTag(tag);
                }
            }
        }
    }
}