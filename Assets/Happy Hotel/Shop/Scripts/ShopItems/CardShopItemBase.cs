using HappyHotel.Card;
using HappyHotel.Equipment.Templates;
using HappyHotel.GameManager;
using HappyHotel.Inventory;
using UnityEngine;

namespace HappyHotel.Shop
{
    // 向背包添加卡牌的商店道具基类
    public abstract class CardShopItemBase : ShopItemBase
    {
        protected int cost;

        public int Cost => cost;

        protected override void OnTemplateSet()
        {
            base.OnTemplateSet();

            if (template is CardTemplate cardTemplate) cost = cardTemplate.cardCost;
        }

        protected override void OnPurchase()
        {
            base.OnPurchase();

            // 检查背包是否存在
            if (CardInventory.Instance == null)
            {
                Debug.LogError("CardInventory未初始化，无法添加物品到卡牌背包");
                return;
            }

            // 调用父类的通用添加逻辑
            var addResult = AddToInventory();

            if (addResult)
            {
                Debug.Log($"成功将 {itemName} 添加到背包");

                // 如果是唯一物品，标记为已获得
                if (template != null && template.isUnique)
                {
                    var uniqueItemManager = UniqueItemManager.Instance;
                    if (uniqueItemManager != null)
                    {
                        uniqueItemManager.MarkItemAsObtained(TypeId.Id);
                        Debug.Log($"标记唯一物品为已获得: {itemName} ({TypeId.Id})");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"无法将 {itemName} 添加到背包，可能背包已满或卡牌无效");
            }
        }

        // 通用的背包添加逻辑，由父类实现
        protected bool AddToInventory()
        {
            // 直接使用 ShopItem 的 TypeId 字符串创建对应的 Card TypeId
            var cardTypeId = Core.Registry.TypeId.Create<CardTypeId>(TypeId.Id);
            if (cardTypeId == null)
            {
                Debug.LogError($"无法创建卡牌TypeId: {TypeId.Id}");
                return false;
            }

            // 获取需要添加的数量
            var count = GetCardCount();

            // 添加指定数量的卡牌到背包
            var successCount = 0;
            for (var i = 0; i < count; i++)
            {
                // 从商店购买的卡牌需要更新备份
                var success = CardInventory.Instance.AddCard(cardTypeId);
                if (success)
                    successCount++;
                else
                    Debug.LogError($"添加第{i + 1}个卡牌失败: {cardTypeId.Id}");
            }

            if (successCount > 0)
                Debug.Log($"成功添加 {successCount}/{count} 个卡牌到背包: {cardTypeId.Id}");
            else
                Debug.LogWarning($"添加卡牌到背包失败: {cardTypeId.Id}");

            return successCount > 0;
        }

        // 子类重写此方法来指定需要添加的卡牌数量（默认返回1）
        protected virtual int GetCardCount()
        {
            return 1;
        }

        // 检查是否可以添加到背包（子类可以重写）
        public virtual bool CanAddToInventory()
        {
            if (CardInventory.Instance == null) return false;
            // 直接允许添加
            return true;
        }
    }
}