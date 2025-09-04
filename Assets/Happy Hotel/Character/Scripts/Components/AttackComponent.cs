using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.Grid.Components;
using HappyHotel.Core.ValueProcessing.Components;
using UnityEngine;

namespace HappyHotel.Character.Components
{
    // 角色攻击组件，处理与其他对象相遇时的攻击逻辑
    [DependsOnComponent(typeof(GridObjectComponent))]
    [DependsOnComponent(typeof(AttackPowerComponent))]
    public class AttackComponent : BehaviorComponentBase
    {
        private AttackPowerComponent attackPowerComponent;
        private GridObjectComponent gridObjectComponent;

        // 攻击目标标签，由CharacterBase设置
        private string[] targetTags;

        public override void OnAttach(BehaviorComponentContainer host)
        {
            base.OnAttach(host);

            // 获取需要的组件
            attackPowerComponent = host.GetBehaviorComponent<AttackPowerComponent>();
            gridObjectComponent = host.GetBehaviorComponent<GridObjectComponent>();

            // 监听网格对象进入事件
            if (gridObjectComponent != null)
            {
                gridObjectComponent.onObjectEnter.AddListener(OnObjectEnter);
            }

            Debug.Log($"{host.gameObject.name} 初始化攻击组件");
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            // 解除事件监听
            if (gridObjectComponent != null)
            {
                gridObjectComponent.onObjectEnter.RemoveListener(OnObjectEnter);
            }
        }

        // 处理对象进入事件
        private void OnObjectEnter(BehaviorComponentContainer other)
        {
            if (other == null || other == host) return;

            // 如果设置了标签，检查目标是否有指定的标签
            if (targetTags != null && targetTags.Length > 0)
            {
                if (!other.HasAnyTag(targetTags)) return;
            }

            // 执行攻击
            PerformAttack(other);
        }

        // 执行攻击
        private void PerformAttack(BehaviorComponentContainer target)
        {
            if (attackPowerComponent == null)
            {
                Debug.LogWarning($"{host.gameObject.name} 没有攻击力组件，无法执行攻击");
                return;
            }

            // 获取目标的血量组件
            var targetHealthComponent = target.GetBehaviorComponent<HitPointValueComponent>();
            if (targetHealthComponent == null)
            {
                Debug.LogWarning($"{target.gameObject.name} 没有血量组件，无法造成伤害");
                return;
            }

            // 获取攻击力
            var attackPower = attackPowerComponent.GetAttackPower();
            if (attackPower <= 0)
            {
                Debug.Log($"{host.gameObject.name} 攻击力为0，无法造成伤害");
                return;
            }

            // 造成伤害（标记来源为攻击）
            targetHealthComponent.TakeDamage(attackPower, HappyHotel.Core.ValueProcessing.DamageSourceType.Attack, host);
            Debug.Log($"{host.gameObject.name} 对 {target.gameObject.name} 造成 {attackPower} 点伤害");

            // 发射命中后事件（供OnHit类Buff使用）
            var hub = host.GetBehaviorComponent<HappyHotel.Core.Combat.AttackEventHub>() ?? host.AddBehaviorComponent<HappyHotel.Core.Combat.AttackEventHub>();
            if (hub != null)
            {
                hub.RaiseAfterDealDamage(new HappyHotel.Core.Combat.AttackEventData
                {
                    Attacker = host,
                    Target = target,
                    BaseDamage = attackPower,
                    FinalDamage = attackPower,
                    SourceType = HappyHotel.Core.ValueProcessing.DamageSourceType.Attack,
                    HitIndex = 0,
                    IsLastHitOfAction = true
                });
            }
        }

        // 设置攻击目标标签
        public void SetTargetTags(string[] tags)
        {
            targetTags = tags;
        }

        // 获取当前攻击力
        public int GetCurrentAttackPower()
        {
            return attackPowerComponent?.GetAttackPower() ?? 0;
        }
    }
}
