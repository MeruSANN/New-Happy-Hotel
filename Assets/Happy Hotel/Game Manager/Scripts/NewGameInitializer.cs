using System;
using HappyHotel.Card;
using HappyHotel.Core.Registry;
using HappyHotel.Core.Singleton;
using HappyHotel.Equipment;
using HappyHotel.Inventory;
using HappyHotel.Shop;
using UnityEngine;

namespace HappyHotel.GameManager
{
    [ManagedSingleton(SceneLoadMode.Exclude, "MapEditScene", "MainMenu", "ShopScene")]
    public class NewGameInitializer : SingletonBase<NewGameInitializer>
    {
        [Header("游戏配置")] [SerializeField] [Tooltip("游戏配置文件路径（Resources文件夹下的相对路径，不包含扩展名）")]
        private string configResourcePath = "GameConfig";

        private CharacterSelectionConfig characterConfig;

        // 初始化新游戏
        public void InitializeNewGame(CharacterSelectionConfig characterSelectionConfig = null)
        {
            Debug.Log("开始初始化新游戏...");

            // 设置角色选择配置
            characterConfig = characterSelectionConfig;

            // 使用ConfigProvider管理配置（不再本地加载）
            if (ConfigProvider.Instance)
            {
                ConfigProvider.Instance.SetResourcePath(configResourcePath);
                ConfigProvider.Instance.EnsureLoaded();
            }

            // 不再向下游传递配置，由各模块从ConfigProvider自行读取

            // 清除血量数据（新游戏重置）
            ClearHealthData();

            // 清空玩家背包
            ClearPlayerInventory();

            // 通过直接添加装备到背包来初始化装备
            InitializeEquipmentsThroughInventory();

            // 通过直接添加卡牌到背包来初始化卡牌
            InitializeCardsThroughInventory();

            // 打乱初始牌库
            ShuffleInitialDeck();

            // 最后设置玩家的最终金币（确保玩家有配置文件中设定的初始金币）
            SetFinalPlayerMoney();

            Debug.Log("新游戏初始化完成");
        }

        // 清空玩家背包
        private void ClearPlayerInventory()
        {
            if (EquipmentInventory.Instance != null) EquipmentInventory.Instance.ClearInventory();
            if (CardInventory.Instance != null) CardInventory.Instance.ClearCards();
            Debug.Log("玩家背包已清空");
        }

        // 添加装备到背包
        private bool AddEquipmentToInventory(EquipmentTypeId equipmentTypeId)
        {
            return EquipmentInventory.Instance?.AddEquipment(equipmentTypeId) ?? false;
        }

        // 添加卡牌到背包
        private bool AddCardToInventory(CardTypeId cardTypeId)
        {
            return CardInventory.Instance?.AddCard(cardTypeId) ?? false;
        }

        // 打乱初始牌库
        private void ShuffleInitialDeck()
        {
            // 初始化卡牌抽取系统
            if (CardDrawManager.Instance != null)
            {
                // 打乱初始牌库
                CardDrawManager.Instance.ShuffleDeck();
                Debug.Log("初始牌库已打乱");
            }
        }

        // 通过直接添加装备到背包来初始化装备
        private void InitializeEquipmentsThroughInventory()
        {
            if (characterConfig == null)
            {
                Debug.LogWarning("角色选择配置为空，跳过装备初始化");
                return;
            }

            if (!characterConfig.IsValid())
            {
                Debug.LogError("角色选择配置无效，跳过装备初始化");
                return;
            }

            if (EquipmentManager.Instance == null)
            {
                Debug.LogError("EquipmentManager实例不存在，无法初始化装备");
                return;
            }

            Debug.Log($"开始直接向背包添加 {characterConfig.GetEquipmentCount()} 个装备...");

            var successCount = 0;
            var failCount = 0;

            foreach (var equipmentId in characterConfig.InitialEquipments)
            {
                if (string.IsNullOrEmpty(equipmentId))
                {
                    Debug.LogWarning("发现空的装备ID，跳过");
                    failCount++;
                    continue;
                }

                try
                {
                    // 直接创建装备TypeId
                    var equipmentTypeId = TypeId.Create<EquipmentTypeId>(equipmentId);

                    // 直接向背包添加装备（使用默认设置）
                    var success = EquipmentInventory.Instance?.AddEquipment(equipmentTypeId) ?? false;

                    if (success)
                    {
                        successCount++;
                        Debug.Log($"成功添加初始装备: {equipmentId}");
                    }
                    else
                    {
                        failCount++;
                        Debug.LogError($"添加初始装备失败: {equipmentId}");
                    }
                }
                catch (Exception e)
                {
                    failCount++;
                    Debug.LogError($"添加装备 {equipmentId} 时发生异常: {e.Message}");
                }
            }

            Debug.Log($"装备初始化完成 - 成功: {successCount}, 失败: {failCount}");
        }

        // 通过直接添加卡牌到背包来初始化卡牌
        private void InitializeCardsThroughInventory()
        {
            if (characterConfig == null)
            {
                Debug.LogWarning("角色选择配置为空，跳过卡牌初始化");
                return;
            }

            if (!characterConfig.IsValid())
            {
                Debug.LogError("角色选择配置无效，跳过卡牌初始化");
                return;
            }

            if (CardManager.Instance == null)
            {
                Debug.LogError("CardManager实例不存在，无法初始化卡牌");
                return;
            }

            Debug.Log($"开始直接向背包添加 {characterConfig.GetCardCount()} 张卡牌...");

            var successCount = 0;
            var failCount = 0;

            foreach (var cardId in characterConfig.InitialCards)
            {
                if (string.IsNullOrEmpty(cardId))
                {
                    Debug.LogWarning("发现空的卡牌ID，跳过");
                    failCount++;
                    continue;
                }

                try
                {
                    var cardTypeId = TypeId.Create<CardTypeId>(cardId);
                    var success = AddCardToInventory(cardTypeId);

                    if (success)
                    {
                        successCount++;
                        Debug.Log($"成功添加初始卡牌: {cardId}");
                    }
                    else
                    {
                        failCount++;
                        Debug.LogError($"添加初始卡牌失败: {cardId}");
                    }
                }
                catch (Exception e)
                {
                    failCount++;
                    Debug.LogError($"添加卡牌 {cardId} 时发生异常: {e.Message}");
                }
            }

            Debug.Log($"卡牌初始化完成 - 成功: {successCount}, 失败: {failCount}");
        }

        // 设置配置文件路径
        public void SetConfigResourcePath(string path)
        {
            configResourcePath = path;
            Debug.Log($"设置配置文件路径: {path}");
        }

        // 获取当前配置文件路径
        public string GetConfigResourcePath()
        {
            return configResourcePath;
        }

        // 获取配置的初始金币数量
        public int GetConfiguredInitialMoney()
        {
            return GetInitialMoney();
        }

        // 设置最终玩家金币
        private void SetFinalPlayerMoney()
        {
            if (ShopMoneyManager.Instance == null)
            {
                Debug.LogError("ShopMoneyManager实例不存在，无法设置最终金币");
                return;
            }

            var finalMoney = GetInitialMoney();
            ShopMoneyManager.Instance.SetCurrentMoney(finalMoney);
            Debug.Log($"新游戏初始化完成，玩家最终金币设置为: {finalMoney}");
        }

        // 清除血量数据
        private void ClearHealthData()
        {
            if (CharacterHealthManager.Instance)
            {
                CharacterHealthManager.Instance.ClearHealthData();
                Debug.Log("新游戏初始化：血量数据已清除");
            }
            else
            {
                Debug.LogWarning("CharacterHealthManager实例不存在，无法清除血量数据");
            }
        }

        // 获取初始金币数量
        private int GetInitialMoney()
        {
            if (characterConfig != null) return characterConfig.InitialMoney;

            // 如果角色配置不存在或无效，使用默认值
            var defaultMoney = 1000;
            Debug.LogWarning($"无法从角色配置获取初始金币，使用默认值: {defaultMoney}");
            return defaultMoney;
        }

		// 敏捷系统已移除
    }
}