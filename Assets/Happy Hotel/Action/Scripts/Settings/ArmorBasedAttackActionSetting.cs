using System.Collections.Generic;
using HappyHotel.Action.Components.Parts;

namespace HappyHotel.Action.Settings
{
    public class ArmorBasedAttackActionSetting : IActionSetting
    {
        private readonly HashSet<string> targetTags;

        public ArmorBasedAttackActionSetting(params string[] tags)
        {
            targetTags = new HashSet<string>(tags ?? new string[0]);
        }

        public void ConfigureAction(ActionBase action)
        {
            if (action is ArmorBasedAttackAction armorBasedAttackAction)
            {
                // 配置攻击组件的目标标签
                var attackComponent = armorBasedAttackAction.GetEntityComponent<AttackEntityComponent>();
                if (attackComponent != null)
                {
                    attackComponent.ClearTargetTags();
                    foreach (var tag in targetTags) attackComponent.AddTargetTag(tag);
                }
            }
        }
    }
}