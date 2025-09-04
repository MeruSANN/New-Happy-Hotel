using System;
using HappyHotel.Shop;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HappyHotel.UI.Shop
{
    // 卡牌商店道具的显示控制脚本
    public class ShopCardDisplayController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI组件引用")] [SerializeField] private Image itemIconImage; // 道具图标

        [SerializeField] private TextMeshProUGUI itemNameText; // 道具名称
        [SerializeField] private TextMeshProUGUI descriptionText; // 道具描述
        [SerializeField] private TextMeshProUGUI costText; // 卡牌费用
        [SerializeField] private TextMeshProUGUI priceText; // 道具价格
        [SerializeField] private Button purchaseButton; // 购买按钮
        [SerializeField] private GameObject soldOutObject; // 售罄状态对象

        // 当前显示的商店道具
        private CardShopItemBase currentCard;

        // 鼠标悬停事件
        public Action<CardShopItemBase> onItemHoverEnter;
        public System.Action onItemHoverExit;

        // 购买按钮点击事件
        public Action<CardShopItemBase> onPurchaseClicked;

        private void Awake()
        {
            // 绑定购买按钮点击事件
            if (purchaseButton != null) purchaseButton.onClick.AddListener(OnPurchaseButtonClicked);
        }

        private void OnDestroy()
        {
            // 清理事件绑定
            if (purchaseButton != null) purchaseButton.onClick.RemoveListener(OnPurchaseButtonClicked);
        }

        // 实现IPointerEnterHandler接口
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (currentCard != null) onItemHoverEnter?.Invoke(currentCard);
        }

        // 实现IPointerExitHandler接口
        public void OnPointerExit(PointerEventData eventData)
        {
            onItemHoverExit?.Invoke();
        }

        // 设置要显示的商店道具
        public void SetShopItem(CardShopItemBase shopItem)
        {
            currentCard = shopItem;
            UpdateDisplay();
        }

        // 更新显示内容
        private void UpdateDisplay()
        {
            if (currentCard == null)
            {
                // 如果没有道具，隐藏所有UI元素
                SetUIElementsActive(false);
                return;
            }

            // 显示UI元素
            SetUIElementsActive(true);

            // 更新图标
            UpdateIcon();

            // 更新名称
            UpdateItemName();

            // 更新描述
            UpdateDescription();

            // 更新费用
            UpdateCost();

            // 更新价格
            UpdatePrice();

            // 更新购买按钮状态
            UpdatePurchaseButton();
        }

        // 更新道具图标
        private void UpdateIcon()
        {
            if (itemIconImage != null)
            {
                if (currentCard.ItemIcon != null)
                {
                    itemIconImage.sprite = currentCard.ItemIcon;
                    itemIconImage.color = Color.white;
                }
                else
                {
                    // 如果没有图标，显示默认颜色或隐藏
                    itemIconImage.sprite = null;
                    itemIconImage.color = Color.gray;
                }
            }
        }

        // 更新道具名称
        private void UpdateItemName()
        {
            if (itemNameText != null) itemNameText.text = currentCard.ItemName;
        }

        // 更新道具描述
        private void UpdateDescription()
        {
            if (descriptionText != null) descriptionText.text = currentCard.GetFormattedDescription();
        }

        // 更新卡牌费用
        private void UpdateCost()
        {
            if (costText != null)
            {
                // 这里需要根据具体的卡牌类型来获取费用
                // 假设ShopItemBase有一个Cost属性，或者需要从具体的卡牌数据中获取
                var cost = currentCard.Cost;
                costText.text = $"{cost}";
            }
        }

        // 更新价格显示
        private void UpdatePrice()
        {
            if (priceText != null) priceText.text = $"{currentCard.Price}";
        }

        // 更新购买按钮状态
        private void UpdatePurchaseButton()
        {
            if (purchaseButton != null)
            {
                // 检查是否可以购买
                var canPurchase = CanPurchaseItem();
                purchaseButton.interactable = canPurchase;
            }
        }

        // 检查是否可以购买道具
        private bool CanPurchaseItem()
        {
            if (currentCard == null)
                return false;

            // 检查金币是否足够
            if (ShopMoneyManager.Instance != null)
                return currentCard.CanPurchase(ShopMoneyManager.Instance.CurrentMoney);

            return false;
        }

        // 设置售罄状态
        public void SetSoldOut(bool isSoldOut)
        {
            if (soldOutObject != null) soldOutObject.SetActive(!isSoldOut);

            if (purchaseButton != null) purchaseButton.interactable = !isSoldOut;

            if (priceText != null)
            {
                if (isSoldOut)
                    priceText.text = "售罄";
                else
                    UpdatePrice();
            }
        }

        // 设置UI元素的激活状态
        private void SetUIElementsActive(bool active)
        {
            if (itemIconImage != null)
                itemIconImage.gameObject.SetActive(active);
            if (itemNameText != null)
                itemNameText.gameObject.SetActive(active);
            if (descriptionText != null)
                descriptionText.gameObject.SetActive(active);
            if (costText != null)
                costText.gameObject.SetActive(active);
            if (priceText != null)
                priceText.gameObject.SetActive(active);
            if (purchaseButton != null)
                purchaseButton.gameObject.SetActive(active);
        }

        // 购买按钮点击处理
        private void OnPurchaseButtonClicked()
        {
            if (currentCard == null)
                return;

            // 触发购买事件
            onPurchaseClicked?.Invoke(currentCard);
        }

        // 执行购买操作
        public bool PurchaseItem()
        {
            if (currentCard == null)
                return false;

            // 使用ShopController进行购买
            if (ShopController.Instance != null)
            {
                var purchaseResult = ShopController.Instance.PurchaseItemWithMoney(currentCard);

                if (purchaseResult)
                    // 购买成功，道具将被从商店移除，UI也会被相应移除
                    // 不需要在这里更新显示，因为整个UI会被销毁
                    Debug.Log($"成功购买卡牌: {currentCard.ItemName}");
                else
                    Debug.LogWarning($"购买卡牌失败: {currentCard.ItemName}");

                return purchaseResult;
            }

            return false;
        }

        // 刷新显示（外部调用，用于更新金币变化等）
        public void RefreshDisplay()
        {
            UpdateDisplay();
        }

        // 获取当前显示的商店道具
        public ShopItemBase GetCurrentShopItem()
        {
            return currentCard;
        }

        // 清空显示
        public void ClearDisplay()
        {
            currentCard = null;
            SetUIElementsActive(false);
        }
    }
}