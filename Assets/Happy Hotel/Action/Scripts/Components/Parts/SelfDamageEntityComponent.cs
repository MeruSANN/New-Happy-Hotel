using HappyHotel.Core.EntityComponent;
using HappyHotel.Core.ValueProcessing.Components;
using UnityEngine;

namespace HappyHotel.Action.Components.Parts
{
    // 自伤组件，对行动执行者造成伤害，优先级15（在攻击组件之后执行）
    [ExecutionPriority(15)]
    public class SelfDamageEntityComponent : EntityComponentBase, IEventListener
    {
        public int SelfDamage { get; private set; } = 1;

        // 实现IEventListener接口，处理事件
        public void OnEvent(EntityComponentEvent evt)
        {
            if (evt.EventName == "Execute" && evt.Data is ActionQueueComponent actionQueue)
                PerformSelfDamage(actionQueue);
        }

        public void SetSelfDamage(int damage)
        {
            SelfDamage = Mathf.Max(0, damage); // 确保伤害不为负数
        }

        // 执行自伤逻辑
        private void PerformSelfDamage(ActionQueueComponent actionQueue)
        {
            // 获取行动发起者
            if (actionQueue == null || !actionQueue.GetHost())
            {
                Debug.LogError("无法获取行动发起者");
                return;
            }

            var selfContainer = actionQueue.GetHost();

            // 检查自己是否有血量组件
            var healthComponent = selfContainer.GetBehaviorComponent<HitPointValueComponent>();
            if (healthComponent == null)
            {
                Debug.LogWarning($"{selfContainer.name} 没有血量组件，无法对自己造成伤害");
                return;
            }

            // 对自己造成伤害
            if (SelfDamage > 0)
            {
                healthComponent.TakeDamage(SelfDamage, selfContainer);
                Debug.Log($"{selfContainer.name} 对自己造成 {SelfDamage} 点伤害");
            }
        }
    }
}