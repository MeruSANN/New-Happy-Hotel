using HappyHotel.Core.EntityComponent;
using HappyHotel.Core.ValueProcessing.Components;
using UnityEngine;

namespace HappyHotel.Action.Components.Parts
{
    // 格挡组件，负责处理格挡逻辑，优先级-10（高优先级，先执行）
    [ExecutionPriority(-10)]
    public class BlockEntityComponent : EntityComponentBase, IEventListener
    {
        public int BlockAmount { get; private set; } = 1;

        // 实现IEventListener接口，处理事件
        public void OnEvent(EntityComponentEvent evt)
        {
            if (evt.EventName == "Execute" && evt.Data is ActionQueueComponent actionQueue) PerformBlock(actionQueue);
        }

        public void SetBlockAmount(int value)
        {
            BlockAmount = Mathf.Max(0, value);
        }

        // 执行格挡逻辑
        private void PerformBlock(ActionQueueComponent actionQueue)
        {
            // 获取行动发起者
            if (actionQueue == null || actionQueue.GetHost() == null)
            {
                Debug.LogError("无法获取行动发起者");
                return;
            }

            var hostBehaviorContainer = actionQueue.GetHost();

            // 获取或添加BlockValueComponent
            var blockComponent = hostBehaviorContainer.GetBehaviorComponent<BlockValueComponent>();
            if (blockComponent == null)
            {
                // 确保先有HitPointValueComponent（BlockValueComponent依赖它）
                var hitPointComponent = hostBehaviorContainer.GetBehaviorComponent<HitPointValueComponent>();
                if (hitPointComponent == null)
                    // 如果没有生命值组件，添加一个默认的
                    hitPointComponent = hostBehaviorContainer.AddBehaviorComponent<HitPointValueComponent>();

                // 现在添加BlockValueComponent
                blockComponent = hostBehaviorContainer.AddBehaviorComponent<BlockValueComponent>();

                // 确保BlockValueComponent正确初始化了依赖关系
                if (blockComponent.BlockValue == null)
                {
                    Debug.LogWarning($"{hostBehaviorContainer.name} BlockValueComponent未正确初始化，尝试重新初始化");
                    // 重新触发OnAttach来确保正确初始化
                    blockComponent.OnAttach(hostBehaviorContainer);
                }
            }

            // 添加格挡值
            if (blockComponent.BlockValue != null)
            {
                blockComponent.AddBlock(BlockAmount);
                Debug.Log($"{hostBehaviorContainer.name} 执行格挡行动，获得 {BlockAmount} 点格挡");
            }
            else
            {
                Debug.LogError($"{hostBehaviorContainer.name} BlockValueComponent的BlockValue为null，无法添加格挡");
            }
        }
    }
}