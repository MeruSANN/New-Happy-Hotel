using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.ValueProcessing.Components;
using UnityEngine;

namespace HappyHotel.Prop.Components
{
    // 生命值流失组件，当道具被触发时直接减少触发者的生命值，绕过格挡和护甲
    public class HealthDrainComponent : BehaviorComponentBase, IEventListener
    {
        [SerializeField] private int drainAmount = 1; // 流失的生命值数量

        public int DrainAmount
        {
            get => drainAmount;
            set => drainAmount = Mathf.Max(0, value); // 确保流失值不为负数
        }

        // 实现IEventListener接口，监听Trigger事件
        public void OnEvent(BehaviorComponentEvent evt)
        {
            if (evt.EventName == "Trigger" && evt.Data is BehaviorComponentContainer triggerer)
                DrainHealthFromTriggerer(triggerer);
        }

        // 流失生命值的方法
        private void DrainHealthFromTriggerer(BehaviorComponentContainer triggerer)
        {
            if (triggerer == null || drainAmount <= 0)
                return;

            // 获取触发者的HitPointValueComponent
            var hitPointComponent = triggerer.GetBehaviorComponent<HitPointValueComponent>();
            if (hitPointComponent == null)
            {
                Debug.LogWarning($"{triggerer.gameObject.name} 没有HitPointValueComponent组件，无法流失生命值");
                return;
            }

            // 直接减少生命值，绕过所有Processor
            var currentHealth = hitPointComponent.CurrentHitPoint;
            var newHealth = Mathf.Max(0, currentHealth - drainAmount);

            // 使用SetCurrentValue直接设置生命值，绕过Processor
            hitPointComponent.HitPointValue.SetCurrentValue(newHealth);

            Debug.Log(
                $"{triggerer.gameObject.name} 通过 {host?.gameObject.name} 流失了 {drainAmount} 点生命值，当前生命值: {newHealth}");
        }

        // 设置流失生命值数量
        public void SetDrainAmount(int amount)
        {
            drainAmount = Mathf.Max(0, amount);
        }

        // 获取流失生命值数量
        public int GetDrainAmount()
        {
            return drainAmount;
        }
    }
}