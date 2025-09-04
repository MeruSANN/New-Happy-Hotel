using System.Collections.Generic;
using HappyHotel.Action.Components.Parts;

namespace HappyHotel.Action.Settings
{
    // 横扫攻击行动设置类
    public class SweepAttackActionSetting : IActionSetting
    {
        private readonly float areaDamageRatio;
        private readonly int mainDamage;
        private readonly HashSet<string> targetTags;

        public SweepAttackActionSetting(int mainDamage = 2, float areaDamageRatio = 0.5f, params string[] tags)
        {
            this.mainDamage = mainDamage;
            this.areaDamageRatio = areaDamageRatio;
            targetTags = new HashSet<string>(tags ?? new string[0]);
        }

        public void ConfigureAction(ActionBase action)
        {
            if (action is SweepAttackAction sweepAttackAction)
            {
                // 配置主攻击组件
                var attackComponent = sweepAttackAction.GetEntityComponent<AttackEntityComponent>();
                if (attackComponent != null)
                {
                    attackComponent.SetDamage(mainDamage);
                    attackComponent.ClearTargetTags();
                    foreach (var tag in targetTags) attackComponent.AddTargetTag(tag);
                }

                // 配置范围攻击组件的标签
                var areaAttackComponent = sweepAttackAction.GetEntityComponent<AreaAttackEntityComponent>();
                if (areaAttackComponent != null)
                {
                    areaAttackComponent.ClearTargetTags();
                    foreach (var tag in targetTags) areaAttackComponent.AddTargetTag(tag);
                }

                // 设置范围伤害比例
                sweepAttackAction.SetAreaDamageRatio(areaDamageRatio);
            }
        }
    }
}