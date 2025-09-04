using System.Collections.Generic;
using HappyHotel.Action.Components.Parts;

namespace HappyHotel.Action.Settings
{
    public class AttackAndArmorActionSetting : IActionSetting
    {
        private readonly int armorAmount;
        private readonly int damage;
        private readonly HashSet<string> targetTags;

        public AttackAndArmorActionSetting(int damage, int armorAmount, params string[] tags)
        {
            this.damage = damage;
            this.armorAmount = armorAmount;
            targetTags = new HashSet<string>(tags ?? new string[0]);
        }

        public void ConfigureAction(ActionBase action)
        {
            if (action is AttackAndArmorAction attackAndArmorAction)
            {
                // 配置攻击组件
                var attackComponent = attackAndArmorAction.GetEntityComponent<AttackEntityComponent>();
                if (attackComponent != null)
                {
                    attackComponent.SetDamage(damage);
                    attackComponent.ClearTargetTags();
                    foreach (var tag in targetTags) attackComponent.AddTargetTag(tag);
                }

                // 配置护甲组件
                var armorComponent = attackAndArmorAction.GetEntityComponent<ArmorEntityComponent>();
                if (armorComponent != null) armorComponent.SetArmorAmount(armorAmount);
            }
        }
    }
}