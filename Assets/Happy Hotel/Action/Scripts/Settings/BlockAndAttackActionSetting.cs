using System.Collections.Generic;
using HappyHotel.Action.Components.Parts;

namespace HappyHotel.Action.Settings
{
    public class BlockAndAttackActionSetting : IActionSetting
    {
        private readonly int blockAmount;
        private readonly int damage;
        private readonly HashSet<string> targetTags;

        public BlockAndAttackActionSetting(int blockAmount, int damage, params string[] tags)
        {
            this.blockAmount = blockAmount;
            this.damage = damage;
            targetTags = new HashSet<string>(tags ?? new string[0]);
        }

        public void ConfigureAction(ActionBase action)
        {
            if (action is BlockAndAttackAction blockAndAttackAction)
            {
                // 配置格挡组件
                var blockComponent = blockAndAttackAction.GetEntityComponent<BlockEntityComponent>();
                if (blockComponent != null) blockComponent.SetBlockAmount(blockAmount);

                // 配置攻击组件
                var attackComponent = blockAndAttackAction.GetEntityComponent<AttackEntityComponent>();
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