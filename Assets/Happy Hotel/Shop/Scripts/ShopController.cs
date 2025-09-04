using System;
using System.Collections.Generic;
using System.Linq;
using HappyHotel.Core.Rarity;
using HappyHotel.Core.Singleton;
using HappyHotel.GameManager;
using HappyHotel.Shop.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace HappyHotel.Shop
{
    // 商店控制器，负责商店刷新、购买等业务逻辑
    [ManagedSingleton(true)]
    public class ShopController : SingletonBase<ShopController>
    {
        // 当前商店中的主动道具列表
        private readonly List<ShopItemBase> cardShopItems = new();

        // 当前商店中的装备列表
        private readonly List<ShopItemBase> equipmentShopItems = new();

        // 稀有度权重配置（现在使用ShopItemSelector的默认配置）
        // 如果需要自定义权重，可以通过参数传递给ShopItemSelector

        // 价格配置
        private readonly Dictionary<Rarity, int> rarityPriceDict = new();

        // 主动道具刷新数量上限

        // 装备刷新数量上限

        // 道具被移除时的事件
        public Action<ShopItemBase> onShopItemRemoved;

        // 商店刷新事件
        public System.Action onShopRefreshed;
        private float priceRandomRangePercent = 0.1f;

        // 属性访问器
        public int MaxCardItems { get; private set; } = 3;

        public int MaxEquipmentItems { get; private set; } = 4;

        public int TotalMaxShopItems => MaxCardItems + MaxEquipmentItems;

        // 获取ShopItemManager实例
        private ShopItemManager ShopItemManager => ShopItemManager.Instance;

        // 获取ShopMoneyManager实例
        private ShopMoneyManager MoneyManager => ShopMoneyManager.Instance;

        // 设置主动道具刷新数量上限
        public void SetMaxActivePropItems(int maxItems)
        {
            MaxCardItems = Mathf.Max(0, maxItems);
        }

        // 设置装备刷新数量上限
        public void SetMaxEquipmentItems(int maxItems)
        {
            MaxEquipmentItems = Mathf.Max(0, maxItems);
        }

        // 添加道具到商店
        public void AddShopItem(ShopItemBase shopItem)
        {
            if (shopItem == null) return;

            // 根据道具类型添加到对应列表
            if (shopItem is CardShopItemBase)
            {
                if (!cardShopItems.Contains(shopItem))
                {
                    cardShopItems.Add(shopItem);
                    Debug.Log($"已添加主动道具到商店: {shopItem.ItemName}");
                }
            }
            else if (shopItem is EquipmentShopItemBase)
            {
                if (!equipmentShopItems.Contains(shopItem))
                {
                    equipmentShopItems.Add(shopItem);
                    Debug.Log($"已添加装备到商店: {shopItem.ItemName}");
                }
            }
            else
            {
                Debug.LogWarning($"未知的商店道具类型: {shopItem.GetType().Name}");
            }
        }

        // 从商店移除道具
        public void RemoveShopItem(ShopItemBase shopItem)
        {
            if (shopItem == null) return;

            var removed = false;

            // 从对应列表中移除
            if (shopItem is CardShopItemBase)
            {
                if (cardShopItems.Contains(shopItem))
                {
                    cardShopItems.Remove(shopItem);
                    removed = true;
                }
            }
            else if (shopItem is EquipmentShopItemBase)
            {
                if (equipmentShopItems.Contains(shopItem))
                {
                    equipmentShopItems.Remove(shopItem);
                    removed = true;
                }
            }

            if (removed)
            {
                ShopItemManager.Remove(shopItem);

                // 触发道具移除事件
                onShopItemRemoved?.Invoke(shopItem);

                Destroy(shopItem.gameObject);
                Debug.Log($"已从商店移除道具: {shopItem.ItemName}");
            }
        }

        // 获取所有商店道具
        public List<ShopItemBase> GetAllShopItems()
        {
            // 清理已销毁的道具
            CleanupDestroyedItems();

            var allItems = new List<ShopItemBase>();
            allItems.AddRange(cardShopItems);
            allItems.AddRange(equipmentShopItems);
            return allItems;
        }

        // 获取所有主动道具
        public List<ShopItemBase> GetAllActivePropItems()
        {
            CleanupDestroyedItems();
            return new List<ShopItemBase>(cardShopItems);
        }

        // 获取所有装备
        public List<ShopItemBase> GetAllEquipmentItems()
        {
            CleanupDestroyedItems();
            return new List<ShopItemBase>(equipmentShopItems);
        }

        // 获取指定类型的商店道具
        public List<T> GetShopItemsOfType<T>() where T : ShopItemBase
        {
            CleanupDestroyedItems();

            var result = new List<T>();
            result.AddRange(cardShopItems.OfType<T>());
            result.AddRange(equipmentShopItems.OfType<T>());
            return result;
        }

        // 根据名称查找商店道具
        public ShopItemBase FindShopItemByName(string itemName)
        {
            CleanupDestroyedItems();

            var activeProp = cardShopItems.FirstOrDefault(item => item.ItemName == itemName);
            if (activeProp != null) return activeProp;

            var equipment = equipmentShopItems.FirstOrDefault(item => item.ItemName == itemName);
            return equipment;
        }

        // 获取可购买的道具
        public List<ShopItemBase> GetPurchasableItems()
        {
            CleanupDestroyedItems();

            var allItems = new List<ShopItemBase>();
            allItems.AddRange(cardShopItems);
            allItems.AddRange(equipmentShopItems);
            return allItems;
        }

        // 清理已销毁的道具
        private void CleanupDestroyedItems()
        {
            cardShopItems.RemoveAll(item => item == null);
            equipmentShopItems.RemoveAll(item => item == null);
        }

        // 清空商店
        public void ClearShop()
        {
            // 清空主动道具
            foreach (var shopItem in cardShopItems.ToList())
                if (shopItem)
                {
                    ShopItemManager.Remove(shopItem);
                    onShopItemRemoved?.Invoke(shopItem);
                    Destroy(shopItem.gameObject);
                }

            cardShopItems.Clear();

            // 清空装备
            foreach (var shopItem in equipmentShopItems.ToList())
                if (shopItem)
                {
                    ShopItemManager.Remove(shopItem);
                    onShopItemRemoved?.Invoke(shopItem);
                    Destroy(shopItem.gameObject);
                }

            equipmentShopItems.Clear();

            Debug.Log("已清空商店");
        }

        // 刷新商店物品
        public void RefreshShop()
        {
            if (!ShopItemManager.IsInitialized)
            {
                Debug.LogError("[ShopController] ShopItemManager未初始化，无法刷新商店");
                return;
            }

            // 确保价格配置已从ConfigProvider应用
            TryApplyPriceConfigFromProvider();

            // 清空当前商店
            ClearShop();

            // 分别刷新主动道具和装备
            RefreshActiveProps();
            RefreshEquipment();

            // 触发商店刷新事件
            onShopRefreshed?.Invoke();
        }

        private void TryApplyPriceConfigFromProvider()
        {
            var provider = ConfigProvider.Instance;
            var cfg = provider ? provider.GetGameConfig() : null;
            if (cfg != null)
                SetPriceConfig(cfg.GetRarityPriceConfig(), cfg.GetPriceRandomRangePercent());
            else
                Debug.LogWarning("[ShopController] 无法从ConfigProvider获取GameConfig，价格配置保持现状");
        }

        // 刷新主动道具
        private void RefreshActiveProps()
        {
            if (MaxCardItems <= 0) return;

            // 使用ShopItemSelector根据稀有度概率选择主动道具
            var selectedTypes = ShopItemSelector.SelectCardsByRarity(MaxCardItems);

            if (selectedTypes.Count == 0)
            {
                Debug.LogWarning("[ShopController] 没有找到可购买的主动道具模板");
                return;
            }

            // 创建选中的主动道具
            foreach (var typeId in selectedTypes)
            {
                var shopItem = ShopItemManager.CreateShopItem(typeId);
                if (shopItem && shopItem is CardShopItemBase)
                    AddShopItem(shopItem);
                else
                    Debug.LogWarning($"[ShopController] 创建主动道具失败或类型不匹配: {typeId}");
            }
        }

        // 刷新装备
        private void RefreshEquipment()
        {
            if (MaxEquipmentItems <= 0) return;

            // 使用ShopItemSelector根据稀有度概率选择装备
            var selectedTypes = ShopItemSelector.SelectEquipmentByRarity(MaxEquipmentItems);

            if (selectedTypes.Count == 0)
            {
                Debug.LogWarning("[ShopController] 没有找到可购买的装备模板");
                return;
            }

            // 创建选中的装备
            foreach (var typeId in selectedTypes)
            {
                var shopItem = ShopItemManager.CreateShopItem(typeId);
                if (shopItem && shopItem is EquipmentShopItemBase)
                    AddShopItem(shopItem);
                else
                    Debug.LogWarning($"[ShopController] 创建装备失败或类型不匹配: {typeId}");
            }
        }

        // 购买道具的便捷方法
        public bool PurchaseItem(ShopItemBase shopItem, int playerMoney)
        {
            if (shopItem && (cardShopItems.Contains(shopItem) || equipmentShopItems.Contains(shopItem)))
                return shopItem.Purchase(playerMoney);

            Debug.LogWarning("尝试购买不存在于商店中的道具");
            return false;
        }

        // 购买道具并扣除金币
        public bool PurchaseItemWithMoney(ShopItemBase shopItem)
        {
            if (shopItem && (cardShopItems.Contains(shopItem) || equipmentShopItems.Contains(shopItem)))
            {
                if (MoneyManager.CurrentMoney >= shopItem.Price)
                {
                    if (shopItem.Purchase(MoneyManager.CurrentMoney))
                    {
                        MoneyManager.SpendMoney(shopItem.Price);
                        Debug.Log($"购买成功！剩余金币: {MoneyManager.CurrentMoney}");

                        // 购买成功后从商店中移除该道具
                        RemoveShopItem(shopItem);

                        return true;
                    }
                }
                else
                {
                    Debug.LogWarning($"金币不足！需要 {shopItem.Price} 金币，当前只有 {MoneyManager.CurrentMoney} 金币");
                }
            }

            return false;
        }

        // 设置价格配置
        public void SetPriceConfig(Dictionary<Rarity, int> priceDict, float randomRangePercent)
        {
            rarityPriceDict.Clear();
            foreach (var kvp in priceDict) rarityPriceDict[kvp.Key] = kvp.Value;

            priceRandomRangePercent = randomRangePercent;
        }

        // 根据稀有度获取基础价格
        public int GetPriceByRarity(Rarity rarity)
        {
            if (rarityPriceDict == null) return 0;

            if (rarityPriceDict.TryGetValue(rarity, out var price)) return price;

            return 0;
        }

        // 获取带浮动的价格
        public int GetRandomizedPrice(Rarity rarity)
        {
            var basePrice = GetPriceByRarity(rarity);
            var min = 1f - priceRandomRangePercent;
            var max = 1f + priceRandomRangePercent;
            var randomFactor = Random.Range(min, max);
            return Mathf.RoundToInt(basePrice * randomFactor);
        }
    }
}