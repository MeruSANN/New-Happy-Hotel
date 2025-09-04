using HappyHotel.Core.EntityComponent;
using HappyHotel.Core.ValueProcessing.Components;
using UnityEngine;

namespace HappyHotel.Action.Components.Parts
{
    // 护甲组件，负责处理护甲逻辑，优先级20（最低优先级，最后执行）
    [ExecutionPriority(20)]
    public class ArmorEntityComponent : EntityComponentBase, IEventListener
    {
        public int ArmorAmount { get; private set; } = 1;

        // 实现IEventListener接口，处理事件
        public void OnEvent(EntityComponentEvent evt)
        {
            if (evt.EventName == "Execute" && evt.Data is ActionQueueComponent actionQueue) PerformArmor(actionQueue);
        }

        public void SetArmorAmount(int value)
        {
            ArmorAmount = Mathf.Max(0, value);
        }

        // 执行护甲逻辑
        private void PerformArmor(ActionQueueComponent actionQueue)
        {
            // 获取行动发起者
            if (actionQueue == null || actionQueue.GetHost() == null)
            {
                Debug.LogError("无法获取行动发起者");
                return;
            }

            var hostBehaviorContainer = actionQueue.GetHost();

            // 获取或添加ArmorValueComponent
            var armorComponent = hostBehaviorContainer.GetBehaviorComponent<ArmorValueComponent>();
            if (armorComponent == null)
            {
                // 现在添加ArmorValueComponent
                armorComponent = hostBehaviorContainer.AddBehaviorComponent<ArmorValueComponent>();

                // 确保ArmorValueComponent正确初始化了依赖关系
                if (armorComponent.ArmorValue == null)
                {
                    Debug.LogWarning($"{hostBehaviorContainer.name} ArmorValueComponent未正确初始化，尝试重新初始化");
                    // 重新触发OnAttach来确保正确初始化
                    armorComponent.OnAttach(hostBehaviorContainer);
                }
            }

            // 添加护甲值
            if (armorComponent.ArmorValue != null)
            {
                armorComponent.AddArmor(ArmorAmount);
                Debug.Log($"{hostBehaviorContainer.name} 执行护甲行动，获得 {ArmorAmount} 点护甲");
            }
            else
            {
                Debug.LogError($"{hostBehaviorContainer.name} ArmorValueComponent的ArmorValue为null，无法添加护甲");
            }
        }
    }
}