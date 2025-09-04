using System.Collections.Generic;
using HappyHotel.Core.Rarity;
using HappyHotel.Reward.Templates;
using HappyHotel.Shop;
using HappyHotel.Shop.Utils;
using HappyHotel.UI;
using UnityEngine;

namespace HappyHotel.Reward
{
    // 混合稀有度选择盒奖励物品
    // 按照配置的稀有度概率权重从所有装备中抽选指定数量的装备供选择
    public class MixedRaritySelectionBoxRewardItem : RewardItemBase
    {
        // 是否已完成本实例的一次性随机
        private bool hasInitializedItems;

        // 添加状态管理
        private bool isWaitingForSelection;
        private MixedRaritySelectionBoxTemplate mixedRarityTemplate;

        // 可选择的商店道具列表
        private List<ShopItemBase> selectableItems = new();

        // 当前本次刷新的数量
        private int selectionCount = 3;

        protected override void OnTemplateSet()
        {
            base.OnTemplateSet();

            if (template is MixedRaritySelectionBoxTemplate mixedTemplate)
            {
                mixedRarityTemplate = mixedTemplate;
                selectionCount = mixedTemplate.selectionCount;
            }
            else
            {
                selectionCount = 3;
            }
        }

        protected override bool OnExecute()
        {
            base.OnExecute();

            // 如果正在等待选择，直接显示UI
            if (isWaitingForSelection)
            {
                ShowSelectionUI();
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
            ShowSelectionUI();

            return false; // 等待用户选择
        }

        // 弹出选择UI
        private void ShowSelectionUI()
        {
            // 查找RaritySelectionUIController
            var uiController = FindObjectOfType<RaritySelectionUIController>();
            if (uiController == null)
            {
                Debug.LogError("未找到RaritySelectionUIController，无法显示选择UI");
                return;
            }

            // 显示选择UI（使用混合稀有度，传入Common作为默认值），传入选择完成回调
            uiController.ShowSelectionUI(selectableItems, Rarity.Common, OnSelectionCompleted);
        }

        // 选择完成回调
        private void OnSelectionCompleted(bool itemSelected)
        {
            Debug.Log($"MixedRaritySelectionBoxRewardItem: 收到选择完成回调，itemSelected={itemSelected}");

            if (itemSelected)
            {
                // 用户选择了道具，完成奖励物品
                Debug.Log("MixedRaritySelectionBoxRewardItem: 用户选择了道具，调用CompleteSelection");
                CompleteSelection();
            }
            else
            {
                // 用户放弃选择，重置状态，保持奖励物品
                Debug.Log("MixedRaritySelectionBoxRewardItem: 用户放弃选择，重置状态");
                isWaitingForSelection = false;
            }
        }

        // 初始化随机道具
        private void InitializeRandomItems()
        {
            // 使用ShopItemSelector选择混合稀有度的装备
            var selectedTypes = ShopItemSelector.SelectEquipmentByRarity(selectionCount);

            // 创建选中的装备实例
            var selectedItems = new List<ShopItemBase>();
            foreach (var typeId in selectedTypes)
            {
                var item = ShopItemManager.Instance.CreateShopItem(typeId);
                if (item != null && item is EquipmentShopItemBase) selectedItems.Add(item);
            }

            // 设置为可选择道具
            selectableItems = selectedItems;

            Debug.Log($"为{GetType().Name}初始化了{selectableItems.Count}个随机混合稀有度装备");
        }
    }
}