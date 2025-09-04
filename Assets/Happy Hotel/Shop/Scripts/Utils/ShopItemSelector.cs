using System.Collections.Generic;
using System.Linq;
using HappyHotel.Core.Rarity;
using HappyHotel.GameManager;
using UnityEngine;

namespace HappyHotel.Shop.Utils
{
    // 商店道具抽选工具类
    // 提供各种道具抽选方法，包括按稀有度权重抽选、按指定稀有度抽选等
    public static class ShopItemSelector
    {
        // 根据稀有度概率选择指定数量的道具
        public static List<ShopItemTypeId> SelectItemsByRarity(List<ShopItemTypeId> availableTypes, int maxCount)
        {
            if (availableTypes == null || availableTypes.Count == 0)
                return new List<ShopItemTypeId>();

            var selectedTypes = new List<ShopItemTypeId>();
            var remainingTypes = new List<ShopItemTypeId>(availableTypes);

            // 按稀有度分组
            var typesByRarity = GroupTypesByRarity(availableTypes);

            // 获取全局概率配置
            var rarityProbabilities = RarityColorManager.Instance.GetRarityConfig().GetAllRarityProbabilities();

            // 选择物品直到达到上限
            for (var i = 0; i < maxCount && remainingTypes.Count > 0; i++)
            {
                var selectedRarity = SelectRarityByProbability(rarityProbabilities);

                // 从对应稀有度中随机选择一个物品
                if (typesByRarity.ContainsKey(selectedRarity) && typesByRarity[selectedRarity].Count > 0)
                {
                    var availableInRarity =
                        typesByRarity[selectedRarity].Where(t => remainingTypes.Contains(t)).ToList();

                    if (availableInRarity.Count > 0)
                    {
                        var selectedType = SelectRandomFromList(availableInRarity);
                        selectedTypes.Add(selectedType);
                        RemoveFromCollections(selectedType, remainingTypes, typesByRarity);
                    }
                    else
                    {
                        // 如果该稀有度没有可用物品，从剩余物品中随机选择
                        var selectedType = SelectRandomFromList(remainingTypes);
                        if (selectedType != null)
                        {
                            selectedTypes.Add(selectedType);
                            RemoveFromCollections(selectedType, remainingTypes, typesByRarity);
                        }
                    }
                }
                else
                {
                    // 如果该稀有度没有物品，从剩余物品中随机选择
                    var selectedType = SelectRandomFromList(remainingTypes);
                    if (selectedType != null)
                    {
                        selectedTypes.Add(selectedType);
                        RemoveFromCollections(selectedType, remainingTypes, typesByRarity);
                    }
                }
            }

            return selectedTypes;
        }

        // 选择指定稀有度的道具
        public static List<ShopItemBase> SelectItemsBySpecificRarity(Rarity targetRarity, int maxCount)
        {
            var shopManager = ShopItemManager.Instance;
            var shopRegistry = ShopItemRegistry.Instance;

            if (shopManager == null || shopRegistry == null)
            {
                Debug.LogWarning("ShopItemManager或ShopItemRegistry未找到，无法选择道具");
                return new List<ShopItemBase>();
            }

            // 获取所有注册的商店道具描述符
            var allDescriptors = shopRegistry.GetAllDescriptors();
            var candidateItems = new List<ShopItemBase>();

            // 为每个描述符创建道具实例并检查稀有度和可购买性
            foreach (var descriptor in allDescriptors)
            {
                var item = shopManager.CreateShopItem(descriptor.TypeId);
                if (item != null && item.Rarity == targetRarity) candidateItems.Add(item);
            }

            // 随机选取maxCount个
            return candidateItems.OrderBy(x => Random.value).Take(maxCount).ToList();
        }

        // 获取所有可购买的道具类型ID
        public static List<ShopItemTypeId> GetAllPurchasableTypes()
        {
            var purchasableTypes = new List<ShopItemTypeId>();
            var shopManager = ShopItemManager.Instance;
            var registry = ShopItemRegistry.Instance;

            if (shopManager == null || registry == null)
            {
                Debug.LogError("无法获取ShopItemManager或ShopItemRegistry");
                return purchasableTypes;
            }

            var allDescriptors = registry.GetAllDescriptors();

            foreach (var descriptor in allDescriptors)
            {
                var template = shopManager.GetTemplate(descriptor.TypeId);
                if (template != null && template.isPurchasable) purchasableTypes.Add(descriptor.TypeId);
            }

            // 过滤掉已获得的唯一物品
            return FilterUniqueItems(purchasableTypes);
        }

        // 获取所有可购买的主动道具类型ID
        public static List<ShopItemTypeId> GetAllPurchasableCardTypes()
        {
            var purchasableTypes = GetAllPurchasableTypes();
            return FilterCardTypes(purchasableTypes);
        }

        // 获取所有可购买的装备类型ID
        public static List<ShopItemTypeId> GetAllPurchasableEquipmentTypes()
        {
            var purchasableTypes = GetAllPurchasableTypes();
            return FilterEquipmentTypes(purchasableTypes);
        }

        // 根据稀有度概率选择指定数量的主动道具
        public static List<ShopItemTypeId> SelectCardsByRarity(int maxCount)
        {
            var activePropTypes = GetAllPurchasableCardTypes();
            return SelectItemsByRarity(activePropTypes, maxCount);
        }

        // 根据稀有度概率选择指定数量的装备
        public static List<ShopItemTypeId> SelectEquipmentByRarity(int maxCount)
        {
            var equipmentTypes = GetAllPurchasableEquipmentTypes();
            return SelectItemsByRarity(equipmentTypes, maxCount);
        }

        // 选择指定稀有度的主动道具
        public static List<CardShopItemBase> SelectCardsBySpecificRarity(Rarity targetRarity, int maxCount)
        {
            var shopManager = ShopItemManager.Instance;
            var shopRegistry = ShopItemRegistry.Instance;

            if (shopManager == null || shopRegistry == null)
            {
                Debug.LogWarning("ShopItemManager或ShopItemRegistry未找到，无法选择主动道具");
                return new List<CardShopItemBase>();
            }

            // 获取所有注册的商店道具描述符
            var allDescriptors = shopRegistry.GetAllDescriptors();
            var candidateItems = new List<CardShopItemBase>();

            // 为每个描述符创建道具实例并检查类型、稀有度和可购买性
            foreach (var descriptor in allDescriptors)
            {
                var item = shopManager.CreateShopItem(descriptor.TypeId);
                if (item != null && item is CardShopItemBase cardShopItem && item.Rarity == targetRarity)
                {
                    candidateItems.Add(cardShopItem);
                }
                else if (item != null)
                {
                    // 销毁不是主动道具的临时创建的道具
                    shopManager.Remove(item);
                    Object.Destroy(item.gameObject);
                }
            }

            // 随机选取maxCount个
            return candidateItems.OrderBy(x => Random.value).Take(maxCount).ToList();
        }

        // 选择指定稀有度的装备
        public static List<ShopItemBase> SelectEquipmentBySpecificRarity(Rarity targetRarity, int maxCount)
        {
            var shopManager = ShopItemManager.Instance;
            var shopRegistry = ShopItemRegistry.Instance;

            if (shopManager == null || shopRegistry == null)
            {
                Debug.LogWarning("ShopItemManager或ShopItemRegistry未找到，无法选择装备");
                return new List<ShopItemBase>();
            }

            // 获取所有注册的商店道具描述符
            var allDescriptors = shopRegistry.GetAllDescriptors();
            var candidateItems = new List<ShopItemBase>();

            // 为每个描述符创建道具实例并检查类型、稀有度和可购买性
            foreach (var descriptor in allDescriptors)
            {
                var item = shopManager.CreateShopItem(descriptor.TypeId);
                if (item != null && item is EquipmentShopItemBase && item.Rarity == targetRarity)
                {
                    candidateItems.Add(item);
                }
                else if (item != null)
                {
                    // 销毁不是装备的临时创建的道具
                    shopManager.Remove(item);
                    Object.Destroy(item.gameObject);
                }
            }

            // 随机选取maxCount个
            return candidateItems.OrderBy(x => Random.value).Take(maxCount).ToList();
        }

        // 过滤主动道具类型
        private static List<ShopItemTypeId> FilterCardTypes(List<ShopItemTypeId> allTypes)
        {
            var activePropTypes = new List<ShopItemTypeId>();
            var shopManager = ShopItemManager.Instance;

            if (shopManager == null)
            {
                Debug.LogError("无法获取ShopItemManager");
                return activePropTypes;
            }

            foreach (var typeId in allTypes)
            {
                var shopItem = shopManager.CreateShopItem(typeId);
                if (shopItem is CardShopItemBase) activePropTypes.Add(typeId);
                // 销毁临时创建的道具
                if (shopItem)
                {
                    shopManager.Remove(shopItem);
                    Object.Destroy(shopItem.gameObject);
                }
            }

            return activePropTypes;
        }

        // 过滤装备类型
        private static List<ShopItemTypeId> FilterEquipmentTypes(List<ShopItemTypeId> allTypes)
        {
            var equipmentTypes = new List<ShopItemTypeId>();
            var shopManager = ShopItemManager.Instance;

            if (shopManager == null)
            {
                Debug.LogError("无法获取ShopItemManager");
                return equipmentTypes;
            }

            foreach (var typeId in allTypes)
            {
                var shopItem = shopManager.CreateShopItem(typeId);
                if (shopItem is EquipmentShopItemBase) equipmentTypes.Add(typeId);
                // 销毁临时创建的道具
                if (shopItem)
                {
                    shopManager.Remove(shopItem);
                    Object.Destroy(shopItem.gameObject);
                }
            }

            return equipmentTypes;
        }

        // 按稀有度分组道具类型
        private static Dictionary<Rarity, List<ShopItemTypeId>> GroupTypesByRarity(List<ShopItemTypeId> availableTypes)
        {
            var typesByRarity = new Dictionary<Rarity, List<ShopItemTypeId>>();
            var shopManager = ShopItemManager.Instance;

            foreach (var typeId in availableTypes)
            {
                var template = shopManager.GetTemplate(typeId);
                if (template != null)
                {
                    if (!typesByRarity.ContainsKey(template.rarity))
                        typesByRarity[template.rarity] = new List<ShopItemTypeId>();
                    typesByRarity[template.rarity].Add(typeId);
                }
            }

            return typesByRarity;
        }

        // 根据概率选择稀有度
        private static Rarity SelectRarityByProbability(Dictionary<Rarity, float> rarityProbabilities)
        {
            var total = rarityProbabilities.Values.Sum();
            var randomValue = Random.Range(0f, total);

            var current = 0f;
            foreach (var kvp in rarityProbabilities)
            {
                current += kvp.Value;
                if (randomValue <= current) return kvp.Key;
            }

            // 默认返回普通稀有度
            return Rarity.Common;
        }

        // 从列表中随机选择一个元素
        private static T SelectRandomFromList<T>(List<T> list)
        {
            if (list == null || list.Count == 0)
                return default;

            var randomIndex = Random.Range(0, list.Count);
            return list[randomIndex];
        }

        // 从各个集合中移除指定的道具类型
        private static void RemoveFromCollections(ShopItemTypeId typeId, List<ShopItemTypeId> remainingTypes,
            Dictionary<Rarity, List<ShopItemTypeId>> typesByRarity)
        {
            remainingTypes.Remove(typeId);

            // 从对应稀有度列表中移除
            var shopManager = ShopItemManager.Instance;
            var template = shopManager.GetTemplate(typeId);
            if (template != null && typesByRarity.ContainsKey(template.rarity))
                typesByRarity[template.rarity].Remove(typeId);
        }

        // 新增：根据当前选择的角色过滤可购买的道具类型ID
        public static List<ShopItemTypeId> GetPurchasableTypesForCurrentCharacter()
        {
            var allPurchasableTypes = GetAllPurchasableTypes();
            return FilterByCurrentCharacter(allPurchasableTypes);
        }

        // 新增：根据当前选择的角色过滤可购买的主动道具类型ID
        public static List<ShopItemTypeId> GetPurchasableCardTypesForCurrentCharacter()
        {
            var activePropTypes = GetAllPurchasableCardTypes();
            return FilterByCurrentCharacter(activePropTypes);
        }

        // 新增：根据当前选择的角色过滤可购买的装备类型ID
        public static List<ShopItemTypeId> GetPurchasableEquipmentTypesForCurrentCharacter()
        {
            var equipmentTypes = GetAllPurchasableEquipmentTypes();
            return FilterByCurrentCharacter(equipmentTypes);
        }

        // 新增：根据稀有度概率选择指定数量的道具（考虑当前角色）
        public static List<ShopItemTypeId> SelectItemsByRarityForCurrentCharacter(int maxCount)
        {
            var availableTypes = GetPurchasableTypesForCurrentCharacter();
            return SelectItemsByRarity(availableTypes, maxCount);
        }

        // 新增：根据稀有度概率选择指定数量的主动道具（考虑当前角色）
        public static List<ShopItemTypeId> SelectCardsByRarityForCurrentCharacter(int maxCount)
        {
            var activePropTypes = GetPurchasableCardTypesForCurrentCharacter();
            return SelectItemsByRarity(activePropTypes, maxCount);
        }

        // 新增：根据稀有度概率选择指定数量的装备（考虑当前角色）
        public static List<ShopItemTypeId> SelectEquipmentByRarityForCurrentCharacter(int maxCount)
        {
            var equipmentTypes = GetPurchasableEquipmentTypesForCurrentCharacter();
            return SelectItemsByRarity(equipmentTypes, maxCount);
        }

        // 过滤掉已获得的唯一物品
        private static List<ShopItemTypeId> FilterUniqueItems(List<ShopItemTypeId> allTypes)
        {
            var filteredTypes = new List<ShopItemTypeId>();
            var shopManager = ShopItemManager.Instance;
            var uniqueItemManager = UniqueItemManager.Instance;

            if (shopManager == null)
            {
                Debug.LogError("无法获取ShopItemManager");
                return filteredTypes;
            }

            if (uniqueItemManager == null)
            {
                Debug.LogWarning("无法获取UniqueItemManager，返回所有道具");
                return allTypes;
            }

            foreach (var typeId in allTypes)
            {
                var template = shopManager.GetTemplate(typeId);
                if (template != null)
                {
                    // 如果是唯一物品且已被获得，则跳过
                    if (template.isUnique && uniqueItemManager.IsItemObtained(typeId.Id)) continue;
                    filteredTypes.Add(typeId);
                }
            }

            Debug.Log($"过滤唯一物品后，可购买的道具数量: {filteredTypes.Count}/{allTypes.Count}");
            return filteredTypes;
        }

        // 新增：根据当前选择的角色过滤道具类型ID列表
        private static List<ShopItemTypeId> FilterByCurrentCharacter(List<ShopItemTypeId> allTypes)
        {
            var filteredTypes = new List<ShopItemTypeId>();
            var shopManager = ShopItemManager.Instance;
            var characterManager = CharacterSelectionManager.Instance;

            if (shopManager == null)
            {
                Debug.LogError("无法获取ShopItemManager");
                return filteredTypes;
            }

            if (characterManager == null)
            {
                Debug.LogWarning("无法获取CharacterSelectionManager，返回所有可购买道具");
                return allTypes;
            }

            // 获取当前选择的角色
            var selectedCharacter = characterManager.GetSelectedCharacter();
            if (selectedCharacter == null)
            {
                Debug.LogWarning("当前没有选择角色，返回所有可购买道具");
                return allTypes;
            }

            var currentCharacterTypeId = selectedCharacter.CharacterTypeId;

            foreach (var typeId in allTypes)
            {
                var template = shopManager.GetTemplate(typeId);
                if (template != null && template.IsCharacterAllowed(currentCharacterTypeId)) filteredTypes.Add(typeId);
            }

            Debug.Log(
                $"角色 {selectedCharacter.CharacterName} ({currentCharacterTypeId}) 可购买的道具数量: {filteredTypes.Count}/{allTypes.Count}");
            return filteredTypes;
        }
    }
}