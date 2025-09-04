using System;
using HappyHotel.Core.Rarity;
using HappyHotel.Shop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HappyHotel.UI
{
    // 卡牌选择界面中单个卡牌显示控制器
    // 用于在选择界面中显示单个卡牌的信息和选择按钮
    public class CardSelectionItemDisplayController : MonoBehaviour
    {
        [Header("UI组件")] [SerializeField] private Image cardIcon; // 卡牌图标

        [SerializeField] private TextMeshProUGUI cardNameText; // 卡牌名称
        [SerializeField] private TextMeshProUGUI cardDescriptionText; // 卡牌描述
        [SerializeField] private TextMeshProUGUI cardCostText; // 卡牌费用
        [SerializeField] private Button selectButton; // 选择按钮

        // 当前显示的卡牌
        private CardShopItemBase currentCard;

        // 选择事件
        public Action<CardShopItemBase> onItemSelected;

        private void Awake()
        {
            // 绑定选择按钮事件
            if (selectButton != null) selectButton.onClick.AddListener(OnSelectButtonClicked);
        }

        private void OnDestroy()
        {
            // 清理事件绑定
            if (selectButton != null) selectButton.onClick.RemoveListener(OnSelectButtonClicked);
        }

        // 设置要显示的卡牌
        public void SetCard(CardShopItemBase card)
        {
            currentCard = card;
            UpdateDisplay();
        }

        // 更新显示内容
        private void UpdateDisplay()
        {
            if (currentCard == null)
            {
                ClearDisplay();
                return;
            }

            // 设置卡牌图标
            if (cardIcon != null)
            {
                cardIcon.sprite = currentCard.ItemIcon;
                cardIcon.gameObject.SetActive(currentCard.ItemIcon != null);
            }

            // 设置卡牌名称
            if (cardNameText != null) cardNameText.text = currentCard.ItemName;

            // 设置卡牌描述
            if (cardDescriptionText != null)
            {
                // 从卡牌中获取格式化描述
                var description = currentCard.GetFormattedDescription();

                // 如果没有描述，显示默认文本
                if (string.IsNullOrEmpty(description)) description = "无描述";

                cardDescriptionText.text = description;
            }

            if (cardCostText != null) cardCostText.text = currentCard.Cost.ToString();

            // 启用选择按钮
            if (selectButton != null) selectButton.interactable = true;
        }

        // 清空显示内容
        private void ClearDisplay()
        {
            if (cardIcon != null)
            {
                cardIcon.sprite = null;
                cardIcon.gameObject.SetActive(false);
            }

            if (cardNameText != null) cardNameText.text = "";

            if (cardDescriptionText != null) cardDescriptionText.text = "";

            if (cardCostText != null)
            {
                cardCostText.text = "";
                cardCostText.gameObject.SetActive(false);
            }

            if (selectButton != null) selectButton.interactable = false;
        }

        // 处理选择按钮点击
        private void OnSelectButtonClicked()
        {
            if (currentCard != null) onItemSelected?.Invoke(currentCard);
        }

        // 获取稀有度颜色
        private Color GetRarityColor(Rarity rarity)
        {
            return RarityColorManager.GetRarityColor(rarity);
        }

        // 设置UI组件引用（用于动态配置）
        public void SetCardIcon(Image icon)
        {
            cardIcon = icon;
        }

        public void SetCardNameText(TextMeshProUGUI nameText)
        {
            cardNameText = nameText;
        }

        public void SetCardDescriptionText(TextMeshProUGUI descriptionText)
        {
            cardDescriptionText = descriptionText;
        }

        public void SetCardCostText(TextMeshProUGUI costText)
        {
            cardCostText = costText;
        }

        public void SetSelectButton(Button button)
        {
            // 清理旧的事件绑定
            if (selectButton != null) selectButton.onClick.RemoveListener(OnSelectButtonClicked);

            selectButton = button;

            // 绑定新的事件
            if (selectButton != null) selectButton.onClick.AddListener(OnSelectButtonClicked);
        }

        // 获取当前显示的卡牌
        public ShopItemBase GetCurrentCard()
        {
            return currentCard;
        }
    }
}