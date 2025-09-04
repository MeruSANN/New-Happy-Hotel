using System.Collections.Generic;
using HappyHotel.Core.EntityComponent;
using HappyHotel.Core.Grid;
using HappyHotel.Core.Grid.Components;
using HappyHotel.Core.ValueProcessing.Components;
using UnityEngine;

namespace HappyHotel.Action.Components.Parts
{
    // 满血额外伤害组件，在攻击前检查目标血量并修改攻击伤害，优先级5（高于攻击组件的10）
    [ExecutionPriority(5)]
    public class FullHealthBonusEntityComponent : EntityComponentBase, IEventListener
    {
        private readonly HashSet<string> targetTags = new();

        public int BonusDamage { get; private set; } = 2;

        // 实现IEventListener接口，处理事件
        public void OnEvent(EntityComponentEvent evt)
        {
            if (evt.EventName == "Execute" && evt.Data is ActionQueueComponent actionQueue)
                CheckTargetHealthAndModifyDamage(actionQueue);
        }

        public void SetBonusDamage(int damage)
        {
            BonusDamage = Mathf.Max(0, damage);
        }

        // 添加目标标签
        public void AddTargetTag(string tag)
        {
            if (!string.IsNullOrEmpty(tag)) targetTags.Add(tag);
        }

        // 移除目标标签
        public void RemoveTargetTag(string tag)
        {
            if (!string.IsNullOrEmpty(tag)) targetTags.Remove(tag);
        }

        // 清空目标标签
        public void ClearTargetTags()
        {
            targetTags.Clear();
        }

        // 检查目标血量并修改攻击伤害
        private void CheckTargetHealthAndModifyDamage(ActionQueueComponent actionQueue)
        {
            // 获取行动发起者
            if (actionQueue == null || actionQueue.GetHost() == null)
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

            var gridPosition = attackerGridComponent.GetGridPosition();

            // 获取目标位置的所有对象
            var containers = GridObjectManager.Instance.GetObjectsAt(gridPosition);

            // 检查是否有满血的目标
            var hasFullHealthTarget = false;

            foreach (var container in containers)
            {
                if (container == attackerContainer) continue;

                // 检查目标是否有血量组件
                var healthComponent = container.GetBehaviorComponent<HitPointValueComponent>();
                if (healthComponent == null) continue;

                // 如果没有指定标签，则检查所有有血量组件的目标
                // 否则检查目标是否具有指定的标签
                if (targetTags.Count == 0 || container.HasAnyTag(targetTags))
                    // 检查目标是否满血
                    if (healthComponent.CurrentHitPoint >= healthComponent.MaxHitPoint)
                    {
                        hasFullHealthTarget = true;
                        Debug.Log(
                            $"发现满血目标: {container.name} (血量: {healthComponent.CurrentHitPoint}/{healthComponent.MaxHitPoint})");
                        break; // 找到一个满血目标就够了
                    }
            }

            // 如果有满血目标，修改攻击组件的伤害
            if (hasFullHealthTarget)
            {
                var attackComponent = GetHost().GetEntityComponent<AttackEntityComponent>();
                if (attackComponent != null)
                {
                    var currentDamage = attackComponent.Damage;
                    var newDamage = currentDamage + BonusDamage;
                    attackComponent.SetDamage(newDamage);

                    Debug.Log($"FullHealthBonusEntityComponent: 发现满血目标，攻击伤害从 {currentDamage} 增加到 {newDamage}");
                }
            }
            else
            {
                Debug.Log("FullHealthBonusEntityComponent: 没有发现满血目标，保持原伤害");
            }
        }
    }
}