using System.Collections.Generic;
using HappyHotel.Core.Rarity;
using HappyHotel.Reward.Templates;
using HappyHotel.Shop;
using HappyHotel.Shop.Utils;
using HappyHotel.UI;
using UnityEngine;

namespace HappyHotel.Reward
{
    // 稀有度选择盒奖励物品基类
    // 被获取时弹出UI，展示3个同稀有度的装备供选择
    public abstract class RaritySelectionBoxRewardItem : RewardItemBase
    {
        // 是否已完成本实例的一次性随机
        private bool hasInitializedItems;

        // 添加状态管理
        protected bool isWaitingForSelection;

        // 可选择的商店道具列表
        protected List<ShopItemBase> selectableItems = new();

        // 当前本次刷新的数量
        protected int selectionCount = 3;

        // 目标稀有度
        protected abstract Rarity TargetRarity { get; }

        protected override void OnTemplateSet()
        {
            base.OnTemplateSet();
            // 尝试从template读取selectionCount字段
            if (template is RaritySelectionBoxTemplate rarityTemplate)
                selectionCount = rarityTemplate.selectionCount;
            else
                selectionCount = 3;
        }

        protected override bool OnExecute()
        {
            base.OnExecute();

            // 如果正在等待选择，直接显示UI
            if (isWaitingForSelection)
            {
                ShowRaritySelectionUI();
                return false; // 继续等待
            }

            // 仅在首次执行时进行随机
            if (!hasInitializedItems)
            {
                InitializeRandomItems();
                hasInitializedItems = true;
            }

            // 设置等待状态并弹出选择UI
            isWaitingForSelection = true;
            ShowRaritySelectionUI();

            return false; // 等待用户选择
        }

        // 弹出稀有度选择UI
        private void ShowRaritySelectionUI()
        {
            // 查找RaritySelectionUIController
            var uiController = FindObjectOfType<RaritySelectionUIController>();
            if (uiController == null)
            {
                Debug.LogError("未找到RaritySelectionUIController，无法显示选择UI");
                return;
            }

            // 显示选择UI，传入选择完成回调
            uiController.ShowSelectionUI(selectableItems, TargetRarity, OnSelectionCompleted);
        }

        // 选择完成回调
        private void OnSelectionCompleted(bool itemSelected)
        {
            Debug.Log($"RaritySelectionBoxRewardItem: 收到选择完成回调，itemSelected={itemSelected}");

            if (itemSelected)
            {
                // 用户选择了道具，完成奖励物品
                Debug.Log("RaritySelectionBoxRewardItem: 用户选择了道具，调用CompleteSelection");
                CompleteSelection();
            }
            else
            {
                // 用户放弃选择，重置状态，保持奖励物品
                Debug.Log("RaritySelectionBoxRewardItem: 用户放弃选择，重置状态");
                isWaitingForSelection = false;
            }
        }

        // 初始化随机道具
        private void InitializeRandomItems()
        {
            // 使用ShopItemSelector选择指定稀有度的装备
            var randomItems = ShopItemSelector.SelectEquipmentBySpecificRarity(TargetRarity, selectionCount);

            // 设置为可选择道具
            selectableItems = randomItems;

            Debug.Log($"为{GetType().Name}初始化了{selectableItems.Count}个随机{TargetRarity}稀有度装备");
        }
    }
}