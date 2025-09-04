using System.Collections.Generic;
using HappyHotel.Action.Components.Parts;
using UnityEngine;

namespace HappyHotel.Action.Settings
{
    public class AttackActionSetting : IActionSetting
    {
        private readonly int damage;
        private readonly HashSet<string> targetTags;

        public AttackActionSetting(int damage, params string[] tags)
        {
            this.damage = damage;
            targetTags = new HashSet<string>(tags ?? new string[0]);
            Debug.Log($"[AttackActionSetting] 构造: 捕获damage={damage}, tags=[{string.Join(",", targetTags)}]");
        }

        public void ConfigureAction(ActionBase action)
        {
            if (action is AttackAction attackAction)
            {
                var attackComponent = attackAction.GetEntityComponent<AttackEntityComponent>();
                if (attackComponent != null)
                {
                    Debug.Log(
                        $"[AttackActionSetting] ConfigureAction: 将设置 action={attackAction.GetType().Name} 的伤害为 {damage}");
                    attackComponent.SetDamage(damage);
                    attackComponent.ClearTargetTags();
                    foreach (var tag in targetTags) attackComponent.AddTargetTag(tag);
                }
            }
        }
    }
}