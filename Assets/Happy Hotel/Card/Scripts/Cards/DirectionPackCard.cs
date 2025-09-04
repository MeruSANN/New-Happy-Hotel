using HappyHotel.Core.Registry;
using HappyHotel.Inventory;
using UnityEngine;

namespace HappyHotel.Card
{
    // 方向包卡牌：使用后向手牌中添加上下左右方向改变卡各一张
    public class DirectionPackCard : CardBase
    {
        public override bool UseCard()
        {
            base.UseCard();

            if (CardInventory.Instance == null)
            {
                Debug.LogError("CardInventory 未初始化");
                return false;
            }

            var inv = CardInventory.Instance;

            // 1) 将四张卡添加到牌库区
            var ids = new[]
            {
                Core.Registry.TypeId.Create<CardTypeId>("DirectionChangerUp"),
                Core.Registry.TypeId.Create<CardTypeId>("DirectionChangerDown"),
                Core.Registry.TypeId.Create<CardTypeId>("DirectionChangerLeft"),
                Core.Registry.TypeId.Create<CardTypeId>("DirectionChangerRight")
            };

            foreach (var id in ids)
            {
                if (id == null)
                {
                    Debug.LogError("无法创建方向改变卡TypeId");
                    continue;
                }
                if (!inv.AddCard(id)) Debug.LogError($"添加卡牌到牌库失败: {id.Id}");
            }

            // 2) 将刚添加的四张卡从牌库移动到手牌
            foreach (var id in ids)
            {
                var card = inv.GetCardByTypeId(id, CardInventory.CardZone.Deck);
                if (card != null)
                {
                    inv.MoveCardToZone(card, CardInventory.CardZone.Hand);
                }
            }

            return true;
        }
    }
}

