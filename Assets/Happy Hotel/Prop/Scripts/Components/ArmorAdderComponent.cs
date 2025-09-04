using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.ValueProcessing.Components;
using UnityEngine;

namespace HappyHotel.Prop.Components
{
    // 护甲添加组件，当道具被触发时直接给触发者添加护甲值
    public class ArmorAdderComponent : BehaviorComponentBase, IEventListener
    {
        [SerializeField] private int armorAmount = 1; // 要添加的护甲值

        public int ArmorAmount
        {
            get => armorAmount;
            set => armorAmount = Mathf.Max(0, value);
        }

        // 实现IEventListener接口，监听Trigger事件
        public void OnEvent(BehaviorComponentEvent evt)
        {
            if (evt.EventName == "Trigger" && evt.Data is BehaviorComponentContainer triggerer)
                AddArmorToTriggerer(triggerer);
        }

        // 添加护甲的方法
        private void AddArmorToTriggerer(BehaviorComponentContainer triggerer)
        {
            if (triggerer == null || armorAmount <= 0)
                return;

            // 获取触发者的护甲值组件
            var armorComponent = triggerer.GetBehaviorComponent<ArmorValueComponent>() ??
                                 triggerer.AddBehaviorComponent<ArmorValueComponent>();

            if (armorComponent != null)
            {
                // 直接添加护甲
                var addedArmor = armorComponent.AddArmor(armorAmount, host);
                Debug.Log($"{triggerer.gameObject.name} 通过 {host?.gameObject.name} 获得了 {addedArmor} 点护甲");
            }
            else
            {
                Debug.LogWarning($"{triggerer.gameObject.name} 没有护甲值组件，无法添加护甲");
            }
        }

        // 设置护甲值
        public void SetArmorAmount(int amount)
        {
            armorAmount = Mathf.Max(0, amount);
        }

        // 获取护甲值
        public int GetArmorAmount()
        {
            return armorAmount;
        }
    }
}