using System.Collections.Generic;
using HappyHotel.Core.EntityComponent;
using HappyHotel.Core.Grid;
using HappyHotel.Core.Grid.Components;
using HappyHotel.Core.Grid.Utils;
using HappyHotel.Core.ValueProcessing.Components;
using UnityEngine;

namespace HappyHotel.Action.Components.Parts
{
    // 范围攻击组件，负责对指定范围内的目标造成伤害，优先级25（在攻击组件之后执行）
    [ExecutionPriority(25)]
    public class AreaAttackEntityComponent : EntityComponentBase, IEventListener
    {
        private readonly HashSet<string> targetTags = new();

        public int AreaDamage { get; private set; } = 1;

        public int AttackRange { get; private set; } = 1;

        // 实现IEventListener接口，处理事件
        public void OnEvent(EntityComponentEvent evt)
        {
            if (evt.EventName == "Execute" && evt.Data is ActionQueueComponent actionQueue)
                PerformAreaAttack(actionQueue);
        }

        public void SetAreaDamage(int damage)
        {
            AreaDamage = Mathf.Max(0, damage);
        }

        public void SetAttackRange(int range)
        {
            AttackRange = Mathf.Max(1, range);
        }

        public void AddTargetTag(string tag)
        {
            if (!string.IsNullOrEmpty(tag)) targetTags.Add(tag);
        }

        public void RemoveTargetTag(string tag)
        {
            if (!string.IsNullOrEmpty(tag)) targetTags.Remove(tag);
        }

        public void ClearTargetTags()
        {
            targetTags.Clear();
        }

        // 执行范围攻击逻辑
        private void PerformAreaAttack(ActionQueueComponent actionQueue)
        {
            // 获取行动发起者的位置
            if (actionQueue == null || !actionQueue.GetHost())
            {
                Debug.LogError("无法获取行动发起者");
                return;
            }

            var attackerContainer = actionQueue.GetHost();
            var attackerGridComponent = attackerContainer.GetBehaviorComponent<GridObjectComponent>();
            if (attackerGridComponent == null)
            {
                Debug.LogError("行动发起者必须有 GridObjectComponent 组件");
                return;
            }

            var centerPosition = attackerGridComponent.GetGridPosition();

            // 获取指定范围内的位置
            var rangePositions = GridRangeUtils.Get8DirectionRange(centerPosition, AttackRange);

            // 对每个范围位置的目标造成范围伤害
            foreach (var position in rangePositions)
            {
                var containers = GridObjectManager.Instance.GetObjectsAt(position);

                foreach (var container in containers)
                {
                    if (container == attackerContainer) continue;

                    // 检查目标是否有血量组件
                    var healthComponent = container.GetBehaviorComponent<HitPointValueComponent>();
                    if (healthComponent == null) continue;

                    // 如果没有指定标签，则攻击所有有血量组件的目标
                    // 否则检查目标是否具有指定的标签
                    if (targetTags.Count == 0 || container.HasAnyTag(targetTags))
                    {
                        healthComponent.TakeDamage(AreaDamage, attackerContainer);
                        Debug.Log($"范围攻击对 {container.name} 造成 {AreaDamage} 点伤害");
                    }
                }
            }
        }
    }
}