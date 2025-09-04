using HappyHotel.Core.EntityComponent;
using HappyHotel.Inventory;
using UnityEngine;
using IEventListener = HappyHotel.Core.EntityComponent.IEventListener;

namespace HappyHotel.Card.Components.Parts
{
    // 抽牌卡牌组件，负责处理抽牌逻辑
    public class DrawCardEntityComponent : EntityComponentBase, IEventListener
    {
        public int DrawAmount { get; private set; } = 1;

        // 实现IEventListener接口，处理UseCard事件
        public void OnEvent(EntityComponentEvent evt)
        {
            if (evt.EventName == "UseCard") ExecuteDrawLogic();
        }

        public void SetDrawAmount(int amount)
        {
            DrawAmount = Mathf.Max(1, amount);
        }

        // 执行抽牌逻辑
        public bool ExecuteDrawLogic()
        {
            if (CardDrawManager.Instance == null)
            {
                Debug.LogError("DrawCardEntityComponent: CardDrawManager不存在，无法抽牌");
                return false;
            }

            var actualDrawn = CardDrawManager.Instance.DrawCards(DrawAmount);

            if (actualDrawn > 0)
            {
                Debug.Log($"DrawCardEntityComponent: 成功抽取了 {actualDrawn} 张卡牌");
                return true;
            }

            Debug.LogWarning("DrawCardEntityComponent: 无法抽取卡牌，可能牌库为空");
            return false;
        }
    }
}