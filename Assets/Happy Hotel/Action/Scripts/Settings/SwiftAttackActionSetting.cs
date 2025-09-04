using System.Collections.Generic;
using HappyHotel.Action.Components.Parts;

namespace HappyHotel.Action.Settings
{
    public class SwiftAttackActionSetting : IActionSetting
    {
        private readonly int damage;
        private readonly HashSet<string> targetTags;

        public SwiftAttackActionSetting(int damage, params string[] tags)
        {
            this.damage = damage;
            targetTags = new HashSet<string>(tags ?? new string[0]);
        }

        public void ConfigureAction(ActionBase action)
        {
            if (action is SwiftAttackAction swiftAttackAction)
            {
                var attackComponent = swiftAttackAction.GetEntityComponent<AttackEntityComponent>();
                if (attackComponent != null)
                {
                    attackComponent.SetDamage(damage);
                    attackComponent.ClearTargetTags();
                    foreach (var tag in targetTags) attackComponent.AddTargetTag(tag);
                }
            }
        }
    }
}