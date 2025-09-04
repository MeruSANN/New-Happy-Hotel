using System.Collections.Generic;
using HappyHotel.Reward.Templates;
using HappyHotel.Shop;
using HappyHotel.Shop.Utils;
using HappyHotel.UI;
using UnityEngine;

namespace HappyHotel.Reward
{
    // 卡牌混合稀有度选择盒奖励物品
    // 按照配置的稀有度概率权重从所有卡牌中抽选指定数量的卡牌供选择
    public class CardMixedRaritySelectionBoxRewardItem : RewardItemBase
    {
        // 是否已完成本实例的一次性随机
        private bool hasInitializedItems;

        // 添加状态管理
        private bool isWaitingForSelection;
        private CardMixedRaritySelectionBoxTemplate mixedRarityTemplate;

        // 可选择的商店道具列表
        private List<CardShopItemBase> selectableItems = new();

        // 当前本次刷新的数量
        private int selectionCount = 3;

        protected override void OnTemplateSet()
        {
            base.OnTemplateSet();

            if (template is CardMixedRaritySelectionBoxTemplate mixedTemplate)
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
                InitializeRandomCards();
                hasInitializedItems = true;
            }

            // 设置等待状态并弹出选择UI
            isWaitingForSelection = true;
            ShowSelectionUI();

            return false; // 等待用户选择
        }

        // 弹出卡牌选择UI
        private void ShowSelectionUI()
        {
            // 查找CardSelectionUIController
            var uiController = FindObjectOfType<CardSelectionUIController>();
            if (uiController == null)
            {
                Debug.LogError("未找到CardSelectionUIController，无法显示卡牌选择UI");
                return;
            }

            // 显示混合稀有度卡牌选择UI，传入选择完成回调
            uiController.ShowMixedRarityCardSelectionUI(selectableItems, OnSelectionCompleted);
        }

        // 选择完成回调
        private void OnSelectionCompleted(bool itemSelected)
        {
            Debug.Log($"CardMixedRaritySelectionBoxRewardItem: 收到选择完成回调，itemSelected={itemSelected}");

            if (itemSelected)
            {
                // 用户选择了道具，完成奖励物品
                Debug.Log("CardMixedRaritySelectionBoxRewardItem: 用户选择了道具，调用CompleteSelection");
                CompleteSelection();
            }
            else
            {
                // 用户放弃选择，重置状态，保持奖励物品
                Debug.Log("CardMixedRaritySelectionBoxRewardItem: 用户放弃选择，重置状态");
                isWaitingForSelection = false;
            }
        }

        // 初始化随机卡牌
        private void InitializeRandomCards()
        {
            // 使用ShopItemSelector选择混合稀有度的卡牌
            var selectedTypes = ShopItemSelector.SelectCardsByRarity(selectionCount);

            // 创建选中的卡牌实例
            var selectedItems = new List<CardShopItemBase>();
            foreach (var typeId in selectedTypes)
            {
                var item = ShopItemManager.Instance.CreateShopItem(typeId);
                if (item != null && item is CardShopItemBase cardShopItem) selectedItems.Add(cardShopItem);
            }

            // 设置为可选择道具
            selectableItems = selectedItems;

            Debug.Log($"为{GetType().Name}初始化了{selectableItems.Count}个随机混合稀有度卡牌");
        }
    }
}