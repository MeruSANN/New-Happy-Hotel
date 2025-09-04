using System;
using System.Collections.Generic;
using HappyHotel.Buff;
using HappyHotel.Buff.Components;
using HappyHotel.Core.EntityComponent;
using HappyHotel.Core.Grid;
using HappyHotel.Core.Grid.Components;
using UnityEngine;
using IEventListener = HappyHotel.Core.EntityComponent.IEventListener;

namespace HappyHotel.Action.Components.Parts
{
    // 目标Buff施加组件，给攻击目标施加buff，优先级50（中等优先级，在攻击之后执行）
    [ExecutionPriority(50)]
    public class TargetBuffApplierEntityComponent : EntityComponentBase, IEventListener
    {
        private readonly HashSet<string> targetTags = new();
        private IBuffSetting buffSetting;
        private string buffTypeString;

        // 实现IEventListener接口，处理事件
        public void OnEvent(EntityComponentEvent evt)
        {
            if (evt.EventName == "Execute" && evt.Data is ActionQueueComponent actionQueue)
                ApplyBuffToTargets(actionQueue);
        }

        // 设置要应用的Buff类型和配置
        public void SetBuffToApply(string buffType, IBuffSetting setting = null)
        {
            buffTypeString = buffType;
            buffSetting = setting;
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

        // 应用Buff到攻击目标
        private void ApplyBuffToTargets(ActionQueueComponent actionQueue)
        {
            if (string.IsNullOrEmpty(buffTypeString))
            {
                Debug.LogError("没有设置要应用的Buff类型");
                return;
            }

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

            // 筛选出符合条件的目标并施加Buff
            foreach (var container in containers)
            {
                if (container == attackerContainer) continue;

                // 获取目标的BuffContainer组件，没有则添加
                var buffContainer = container.GetBehaviorComponent<BuffContainer>() ??
                                    container.AddBehaviorComponent<BuffContainer>();

                // 如果没有指定标签，则给所有有BuffContainer组件的目标施加buff
                // 否则检查目标是否具有指定的标签
                if (targetTags.Count == 0 || container.HasAnyTag(targetTags))
                {
                    // 创建新的Buff实例
                    var buffToApply = CreateBuffInstance();
                    if (buffToApply != null)
                    {
                        buffContainer.AddBuff(buffToApply);
                        Debug.Log($"向 {container.name} 施加了 {buffToApply.GetType().Name}");
                    }
                }
            }
        }

        // 通过BuffManager创建Buff实例
        private BuffBase CreateBuffInstance()
        {
            if (!BuffManager.Instance.IsInitialized)
            {
                Debug.LogError("BuffManager尚未初始化");
                return null;
            }

            try
            {
                // 使用BuffManager创建Buff实例
                var buff = BuffManager.Instance.Create(buffTypeString, buffSetting);
                return buff;
            }
            catch (Exception e)
            {
                Debug.LogError($"创建Buff实例失败: {e.Message}");
                return null;
            }
        }

        // 获取当前设置的Buff类型字符串
        public string GetBuffTypeString()
        {
            return buffTypeString;
        }

        // 获取当前的Buff设置
        public IBuffSetting GetBuffSetting()
        {
            return buffSetting;
        }

        // 清理资源
        public override void OnDestroy()
        {
            base.OnDestroy();
            buffTypeString = null;
            buffSetting = null;
            targetTags.Clear();
        }
    }
}