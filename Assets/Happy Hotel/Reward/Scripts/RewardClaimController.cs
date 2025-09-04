using System;
using System.Collections.Generic;
using System.Linq;
using HappyHotel.Core.Singleton;
using HappyHotel.GameManager;
using HappyHotel.Map;
using HappyHotel.Map.Data;
using HappyHotel.Reward.Settings;
using UnityEngine;
using Random = UnityEngine.Random;

namespace HappyHotel.Reward
{
    // 奖励控制器，负责奖励刷新、获取等业务逻辑
    [ManagedSingleton(true)]
    public class RewardClaimController : SingletonBase<RewardClaimController>
    {
        // 新增：动态添加的奖励物品列表（在关卡进行中添加的额外奖励）
        private readonly List<RewardItemBase> dynamicRewardItems = new();

        // 当前奖励中的物品列表
        private readonly List<RewardItemBase> rewardItems = new();

        // 新增：动态奖励添加事件
        public Action<RewardItemBase> onDynamicRewardAdded;

        // 物品被获取时的事件
        public Action<RewardItemBase> onRewardItemClaimed;

        // 奖励刷新事件
        public System.Action onRewardRefreshed;

        // 获取RewardItemManager实例
        private RewardItemManager RewardItemManager => RewardItemManager.Instance;

        // 添加奖励物品
        public void AddRewardItem(RewardItemBase rewardItem)
        {
            if (rewardItem && !rewardItems.Contains(rewardItem))
            {
                rewardItems.Add(rewardItem);
                Debug.Log($"已添加奖励物品: {rewardItem.ItemName}");
            }
        }

        // 获取奖励物品
        public void ClaimRewardItem(RewardItemBase rewardItem)
        {
            if (rewardItem && rewardItems.Contains(rewardItem))
            {
                // 执行奖励逻辑，获取是否立即完成
                var isCompleted = rewardItem.Execute();

                if (isCompleted)
                    // 立即完成，删除奖励物品
                    RemoveRewardItem(rewardItem);
                else
                    // 等待用户选择，不删除奖励物品
                    Debug.Log($"奖励物品 {rewardItem.ItemName} 等待用户选择");
            }
        }

        // 添加选择完成处理方法
        public void OnRewardItemSelectionCompleted(RewardItemBase rewardItem)
        {
            Debug.Log($"RewardClaimController: OnRewardItemSelectionCompleted被调用，奖励物品={rewardItem?.ItemName}");

            if (rewardItem && rewardItems.Contains(rewardItem))
            {
                Debug.Log("RewardClaimController: 奖励物品在列表中，开始删除");
                RemoveRewardItem(rewardItem);
            }
            else
            {
                Debug.LogWarning(
                    $"RewardClaimController: 奖励物品不在列表中或为null，rewardItem={rewardItem?.ItemName}, 在列表中={rewardItems.Contains(rewardItem)}");
            }
        }

        // 提取删除逻辑为独立方法
        private void RemoveRewardItem(RewardItemBase rewardItem)
        {
            Debug.Log($"RewardClaimController: RemoveRewardItem开始执行，奖励物品={rewardItem?.ItemName}");

            // 从奖励列表中移除
            var removedFromList = rewardItems.Remove(rewardItem);
            Debug.Log($"RewardClaimController: 从列表中移除结果={removedFromList}");

            RewardItemManager.Remove(rewardItem);
            Debug.Log("RewardClaimController: 从RewardItemManager移除完成");

            // 触发物品获取事件
            onRewardItemClaimed?.Invoke(rewardItem);
            Debug.Log("RewardClaimController: 触发物品获取事件完成");

            Destroy(rewardItem.gameObject);
            Debug.Log($"已获取奖励物品: {rewardItem.ItemName}");
        }

        // 获取所有奖励物品
        public List<RewardItemBase> GetAllRewardItems()
        {
            // 清理已销毁的物品
            rewardItems.RemoveAll(item => item == null);
            return new List<RewardItemBase>(rewardItems);
        }

        // 获取指定类型的奖励物品
        public List<T> GetRewardItemsOfType<T>() where T : RewardItemBase
        {
            rewardItems.RemoveAll(item => item == null);
            return rewardItems.OfType<T>().ToList();
        }

        // 新增：动态添加奖励物品（在关卡进行中调用）
        public void AddDynamicRewardItem(RewardItemBase rewardItem)
        {
            if (rewardItem && !dynamicRewardItems.Contains(rewardItem))
            {
                dynamicRewardItems.Add(rewardItem);
                Debug.Log($"已动态添加奖励物品: {rewardItem.ItemName}");

                // 触发动态奖励添加事件
                onDynamicRewardAdded?.Invoke(rewardItem);
            }
        }

        // 新增：动态添加奖励物品（通过TypeId）
        public void AddDynamicRewardItem(string rewardTypeId, IRewardItemSetting setting = null)
        {
            var rewardItem = CreateRewardItemByTypeId(rewardTypeId, setting);
            if (rewardItem != null) AddDynamicRewardItem(rewardItem);
        }

        // 新增：获取所有动态奖励物品
        public List<RewardItemBase> GetAllDynamicRewardItems()
        {
            // 清理已销毁的物品
            dynamicRewardItems.RemoveAll(item => item == null);
            return new List<RewardItemBase>(dynamicRewardItems);
        }

        // 新增：清空动态奖励（进入新关卡时调用）
        public void ClearDynamicRewards()
        {
            foreach (var rewardItem in dynamicRewardItems.ToList())
                if (rewardItem)
                {
                    RewardItemManager.Remove(rewardItem);
                    Destroy(rewardItem.gameObject);
                }

            dynamicRewardItems.Clear();
            Debug.Log("已清空动态奖励");
        }

        // 新增：静默清空动态奖励（不触发事件）
        public void ClearDynamicRewardsSilently()
        {
            foreach (var rewardItem in dynamicRewardItems.ToList())
                if (rewardItem)
                {
                    RewardItemManager.Remove(rewardItem);
                    Destroy(rewardItem.gameObject);
                }

            dynamicRewardItems.Clear();
            Debug.Log("已静默清空动态奖励");
        }

        // 根据名称查找奖励物品
        public RewardItemBase FindRewardItemByName(string itemName)
        {
            rewardItems.RemoveAll(item => item == null);
            return rewardItems.FirstOrDefault(item => item.ItemName == itemName);
        }

        // 清空奖励
        public void ClearRewards()
        {
            foreach (var rewardItem in rewardItems.ToList())
                if (rewardItem)
                {
                    RewardItemManager.Remove(rewardItem);

                    // 触发物品获取事件
                    onRewardItemClaimed?.Invoke(rewardItem);

                    Destroy(rewardItem.gameObject);
                }

            rewardItems.Clear();
            Debug.Log("已清空奖励");
        }

        // 静默清空奖励（不触发事件）
        public void ClearRewardsSilently()
        {
            foreach (var rewardItem in rewardItems.ToList())
                if (rewardItem)
                {
                    RewardItemManager.Remove(rewardItem);
                    Destroy(rewardItem.gameObject);
                }

            rewardItems.Clear();
            Debug.Log("已静默清空剩余奖励");
        }

        // 为关卡完成刷新奖励物品（根据关卡类型配置）
        public void RefreshRewardsForLevelCompletion()
        {
            if (!RewardItemManager.IsInitialized)
            {
                Debug.LogError("RewardItemManager未初始化，无法刷新奖励");
                return;
            }

            // 清空当前奖励
            ClearRewards();

            // 获取当前关卡信息
            var mapData = GetCurrentMapData();
            var gameConfig = GetGameConfig();

            if (mapData == null || gameConfig == null)
            {
                Debug.LogError("无法获取地图数据或游戏配置，无法刷新奖励");
                return;
            }

            // 获取关卡奖励配置
            var levelRewardConfig = gameConfig.GetLevelRewardConfig(mapData.levelType);
            if (levelRewardConfig == null)
            {
                Debug.LogWarning($"未找到关卡类型 {mapData.levelType} 的奖励配置，使用默认配置");
                // 使用默认配置
                levelRewardConfig = new LevelRewardConfig
                {
                    levelType = "Normal",
                    rewardItemTypeIds = new System.Collections.Generic.List<string> { "CoinReward", "MixedRaritySelectionBox" }
                };
            }

            // 处理所有奖励物品
            foreach (var rewardTypeId in levelRewardConfig.rewardItemTypeIds)
                if (!string.IsNullOrEmpty(rewardTypeId))
                {
                    RewardItemBase rewardItem = null;

                    // 特殊处理金币奖励
                    if (rewardTypeId == "CoinReward")
                        rewardItem = CreateCoinRewardItemForLevel(mapData, gameConfig);
                    else
                        // 创建其他类型的奖励物品
                        rewardItem = CreateRewardItemByTypeId(rewardTypeId);

                    if (rewardItem != null)
                    {
                        AddRewardItem(rewardItem);
                        Debug.Log($"已添加奖励物品: {rewardTypeId}");
                    }
                    else
                    {
                        Debug.LogError($"无法创建奖励物品: {rewardTypeId}");
                    }
                }

            // 新增：添加动态奖励物品
            AddDynamicRewardsToMainRewards();

            Debug.Log(
                $"关卡完成奖励已刷新，关卡类型: {mapData.levelType}, 难度: {mapData.levelDifficulty}, 当前有 {rewardItems.Count} 个物品");

            // 触发奖励刷新事件
            onRewardRefreshed?.Invoke();
        }

        // 获取当前奖励物品数量
        public int GetRewardItemCount()
        {
            rewardItems.RemoveAll(item => item == null);
            return rewardItems.Count;
        }

        // 获取当前地图数据
        private MapData GetCurrentMapData()
        {
            if (MapStorageManager.Instance != null) return MapStorageManager.Instance.GetCurrentMapData();
            return null;
        }

        // 获取当前回合数
        private int GetCurrentTurnNumber()
        {
            if (TurnManager.Instance != null) return TurnManager.Instance.GetCurrentTurn();
            return 1; // 默认为第1回合
        }

        // 获取游戏配置（统一从ConfigProvider获取）
        private GameConfig GetGameConfig()
        {
            return ConfigProvider.Instance
                ? ConfigProvider.Instance.GetGameConfig()
                : null;
        }

        // 计算最终金币奖励（应用倍数和随机因子）
        private int CalculateFinalCoinReward(int baseCoinReward, int completionTurn)
        {
            var gameConfig = GetGameConfig();
            if (gameConfig != null) return gameConfig.CalculateFinalCoinReward(baseCoinReward, completionTurn);

            // 如果无法获取配置，使用默认计算（无倍数，仅随机因子）
            var randomFactor = Random.Range(0.9f, 1.1f);
            return Mathf.RoundToInt(baseCoinReward * randomFactor);
        }

        // 创建金币奖励物品
        private RewardItemBase CreateCoinRewardItem(int coinAmount)
        {
            try
            {
                // 创建设置
                var setting = new CoinRewardSetting(coinAmount);

                // 使用RewardItemManager创建物品
                var coinRewardItem = RewardItemManager.Instance.CreateRewardItem("CoinReward", setting);

                return coinRewardItem;
            }
            catch (Exception e)
            {
                Debug.LogError($"创建金币奖励物品失败: {e.Message}");
                return null;
            }
        }

        // 为关卡创建金币奖励物品
        private RewardItemBase CreateCoinRewardItemForLevel(MapData mapData, GameConfig gameConfig)
        {
            // 获取基础金币奖励
            var baseCoinReward = gameConfig.GetBaseCoinReward(mapData.levelType, mapData.levelDifficulty);

            if (baseCoinReward <= 0)
            {
                Debug.Log($"关卡类型 {mapData.levelType} 难度 {mapData.levelDifficulty} 没有配置金币奖励");
                return null;
            }

            // 获取当前回合数
            var currentTurn = GetCurrentTurnNumber();

            // 计算最终金币奖励（应用倍数和随机因子）
            var finalCoinReward = CalculateFinalCoinReward(baseCoinReward, currentTurn);

            // 创建金币奖励物品
            var coinRewardItem = CreateCoinRewardItem(finalCoinReward);
            if (coinRewardItem != null)
                Debug.Log(
                    $"已创建金币奖励物品，关卡类型: {mapData.levelType}, 难度: {mapData.levelDifficulty}, 基础奖励: {baseCoinReward}, 第{currentTurn}回合通关, 最终奖励: {finalCoinReward}");

            return coinRewardItem;
        }

        // 新增：将动态奖励添加到主奖励列表中
        private void AddDynamicRewardsToMainRewards()
        {
            // 清理已销毁的动态奖励物品
            dynamicRewardItems.RemoveAll(item => item == null);

            foreach (var dynamicReward in dynamicRewardItems.ToList())
                if (dynamicReward != null)
                {
                    // 将动态奖励添加到主奖励列表
                    AddRewardItem(dynamicReward);
                    Debug.Log($"已将动态奖励添加到主奖励列表: {dynamicReward.ItemName}");
                }

            // 清空动态奖励列表（但不销毁物品，因为它们现在在主奖励列表中）
            dynamicRewardItems.Clear();
        }

        // 创建指定类型的奖励物品（支持设置参数）
        private RewardItemBase CreateRewardItemByTypeId(string typeId, IRewardItemSetting setting = null)
        {
            try
            {
                return RewardItemManager.Instance.CreateRewardItem(typeId, setting);
            }
            catch (Exception e)
            {
                Debug.LogError($"创建奖励物品失败 {typeId}: {e.Message}");
                return null;
            }
        }
    }
}