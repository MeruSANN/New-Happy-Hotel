using System.Collections.Generic;
using HappyHotel.Core.EntityComponent;
using HappyHotel.Core.Grid;
using HappyHotel.Core.Grid.Components;
using HappyHotel.Core.ValueProcessing;
using HappyHotel.Core.ValueProcessing.Components;
using HappyHotel.Core.ValueProcessing.Processors;
using UnityEngine;

namespace HappyHotel.Action.Components.Parts
{
    // 攻击组件，负责处理攻击逻辑，优先级10（低优先级，后执行）
    [ExecutionPriority(10)]
    public class AttackEntityComponent : EntityComponentBase, IEventListener
    {
        private readonly HashSet<string> targetTags = new();
        private AttackValue attackValue;

        public int Damage => attackValue?.GetFinalDamage() ?? 1;

        // 初始化攻击值
        public override void OnAttach(EntityComponentContainer host)
        {
            base.OnAttach(host);

            attackValue = new AttackValue();
            attackValue.Initialize(GetHost()); // 默认伤害为1
        }

        // 实现IEventListener接口，处理事件
        public void OnEvent(EntityComponentEvent evt)
        {
            if (evt.EventName == "Execute" && evt.Data is ActionQueueComponent actionQueue) PerformAttack(actionQueue);
        }

        public void SetDamage(int value)
        {
            var beforeRaw = attackValue?.CurrentValue ?? 0;
            var beforeFinal = attackValue?.GetFinalDamage() ?? 0;
            attackValue?.SetBaseDamage(value);
            var afterRaw = attackValue?.CurrentValue ?? 0;
            var afterFinal = attackValue?.GetFinalDamage() ?? 0;
            Debug.Log($"[AttackEntityComponent] SetDamage: {beforeRaw}/{beforeFinal} -> {afterRaw}/{afterFinal}");
        }

        // 注册伤害处理器
        public void RegisterDamageProcessor(IValueProcessor processor)
        {
            attackValue?.RegisterProcessor(processor);
        }

        // 注销伤害处理器
        public void UnregisterDamageProcessor(IValueProcessor processor)
        {
            attackValue?.UnregisterProcessor(processor);
        }

        // 获取攻击值对象（供外部访问）
        public AttackValue GetAttackValue()
        {
            return attackValue;
        }

        // 清理资源
        public override void OnDestroy()
        {
            base.OnDestroy();

            attackValue?.Dispose();
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

        // 执行攻击逻辑
        public void PerformAttack(ActionQueueComponent actionQueue)
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

            var gridPosition = attackerGridComponent.GetGridPosition();

            // 获取目标位置的所有对象
            var containers = GridObjectManager.Instance.GetObjectsAt(gridPosition);

            // 筛选出符合条件的对象并造成伤害
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
                    var finalDamage = Damage; // 使用属性获取最终伤害，这会触发处理器
                    healthComponent.TakeDamage(finalDamage, attackerContainer);
                    Debug.Log($"对 {container.name} 造成 {finalDamage} 点伤害");
                }
            }
        }
    }
}