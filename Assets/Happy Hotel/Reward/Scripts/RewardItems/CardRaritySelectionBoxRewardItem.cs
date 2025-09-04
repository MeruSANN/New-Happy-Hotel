using System.Collections.Generic;
using HappyHotel.Reward.Templates;
using HappyHotel.Shop;
using HappyHotel.Shop.Utils;
using HappyHotel.UI;
using UnityEngine;

namespace HappyHotel.Reward
{
    // 卡牌稀有度选择盒奖励物品
    // 被获取时弹出UI，展示3个同稀有度的卡牌供选择
    public class CardRaritySelectionBoxRewardItem : RewardItemBase
    {
        // 添加状态管理
        private bool isWaitingForSelection;
        private CardRaritySelectionBoxTemplate rarityTemplate;

        // 可选择的商店道具列表
        private List<CardShopItemBase> selectableItems = new();

        // 当前本次刷新的数量
        private int selectionCount = 3;

        protected override void OnTemplateSet()
        {
            base.OnTemplateSet();

            if (template is CardRaritySelectionBoxTemplate rarityTemp)
            {
                rarityTemplate = rarityTemp;
                selectionCount = rarityTemp.selectionCount;
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
                ShowRaritySelectionUI();
                return false; // 继续等待
            }

            // 初始化随机卡牌
            InitializeRandomCards();

            // 设置等待状态
            isWaitingForSelection = true;

            // 弹出选择UI
            ShowRaritySelectionUI();

            return false; // 等待用户选择
        }

        // 弹出卡牌选择UI
        private void ShowRaritySelectionUI()
        {
            // 查找CardSelectionUIController
            var uiController = FindObjectOfType<CardSelectionUIController>();
            if (uiController == null)
            {
                Debug.LogError("未找到CardSelectionUIController，无法显示卡牌选择UI");
                return;
            }

            // 显示卡牌选择UI，传入选择完成回调
            uiController.ShowCardSelectionUI(selectableItems, rarityTemplate.targetRarity, OnSelectionCompleted);
        }

        // 选择完成回调
        private void OnSelectionCompleted(bool itemSelected)
        {
            Debug.Log($"CardRaritySelectionBoxRewardItem: 收到选择完成回调，itemSelected={itemSelected}");

            if (itemSelected)
            {
                // 用户选择了道具，完成奖励物品
                Debug.Log("CardRaritySelectionBoxRewardItem: 用户选择了道具，调用CompleteSelection");
                CompleteSelection();
            }
            else
            {
                // 用户放弃选择，重置状态，保持奖励物品
                Debug.Log("CardRaritySelectionBoxRewardItem: 用户放弃选择，重置状态");
                isWaitingForSelection = false;
            }
        }

        // 初始化随机卡牌
        private void InitializeRandomCards()
        {
            // 使用ShopItemSelector选择指定稀有度的卡牌
            var randomCards = ShopItemSelector.SelectCardsBySpecificRarity(rarityTemplate.targetRarity, selectionCount);

            // 设置为可选择道具
            selectableItems = randomCards;

            Debug.Log($"为{GetType().Name}初始化了{selectableItems.Count}个随机{rarityTemplate.targetRarity}稀有度卡牌");
        }
    }
}