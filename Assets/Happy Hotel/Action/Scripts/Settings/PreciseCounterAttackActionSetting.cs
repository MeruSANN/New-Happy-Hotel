using System.Collections.Generic;
using HappyHotel.Action.Components.Parts;

namespace HappyHotel.Action.Settings
{
    public class PreciseCounterAttackActionSetting : IActionSetting
    {
        private readonly HashSet<string> targetTags;

        public PreciseCounterAttackActionSetting(params string[] tags)
        {
            targetTags = new HashSet<string>(tags ?? new string[0]);
        }

        public void ConfigureAction(ActionBase action)
        {
            if (action is PreciseCounterAttackAction preciseCounterAttackAction)
            {
                // 配置攻击目标标签
                var attackComponent = preciseCounterAttackAction.GetEntityComponent<AttackEntityComponent>();
                if (attackComponent != null)
                {
                    attackComponent.ClearTargetTags();
                    foreach (var tag in targetTags) attackComponent.AddTargetTag(tag);
                }
            }
        }
    }
}