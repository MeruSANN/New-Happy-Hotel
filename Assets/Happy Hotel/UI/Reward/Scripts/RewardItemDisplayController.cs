using System;
using HappyHotel.Core.Rarity;
using HappyHotel.Reward;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HappyHotel.UI.Reward
{
    // 单个奖励道具的显示控制脚本
    public class RewardItemDisplayController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI组件引用")] [SerializeField] private Image itemIconImage; // 道具图标

        [SerializeField] private TextMeshProUGUI itemDescriptionText; // 道具描述
        [SerializeField] private Button claimButton; // 获取按钮

        // 当前显示的奖励道具
        private RewardItemBase currentRewardItem;

        // 获取按钮点击事件
        public Action<RewardItemBase> OnClaimClicked;

        // 鼠标悬停事件
        public Action<RewardItemBase> OnItemHoverEnter;
        public System.Action OnItemHoverExit;

        private void Awake()
        {
            // 绑定获取按钮点击事件
            if (claimButton != null) claimButton.onClick.AddListener(OnClaimButtonClicked);
        }

        private void OnDestroy()
        {
            // 清理事件绑定
            if (claimButton != null) claimButton.onClick.RemoveListener(OnClaimButtonClicked);
        }

        // 实现IPointerEnterHandler接口
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (currentRewardItem != null) OnItemHoverEnter?.Invoke(currentRewardItem);
        }

        // 实现IPointerExitHandler接口
        public void OnPointerExit(PointerEventData eventData)
        {
            OnItemHoverExit?.Invoke();
        }

        // 设置要显示的奖励道具
        public void SetRewardItem(RewardItemBase rewardItem)
        {
            currentRewardItem = rewardItem;
            UpdateDisplay();
        }

        // 更新显示内容
        private void UpdateDisplay()
        {
            if (currentRewardItem == null)
            {
                // 如果没有道具，隐藏所有UI元素
                SetUIElementsActive(false);
                return;
            }

            // 显示UI元素
            SetUIElementsActive(true);

            // 更新图标
            UpdateIcon();

            // 更新描述
            UpdateItemDescription();

            // 更新获取按钮状态
            UpdateClaimButton();
        }

        // 更新道具图标
        private void UpdateIcon()
        {
            if (itemIconImage != null)
            {
                if (currentRewardItem.ItemIcon != null)
                {
                    itemIconImage.sprite = currentRewardItem.ItemIcon;
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

        // 更新道具描述
        private void UpdateItemDescription()
        {
            if (itemDescriptionText != null)
            {
                // 从物品中获取格式化描述
                var description = currentRewardItem.GetFormattedDescription();

                // 如果没有描述，显示默认文本
                if (string.IsNullOrEmpty(description)) description = "无描述";

                itemDescriptionText.text = description;

                // 描述文字也使用稀有度颜色
                var rarityColor = RarityColorManager.GetRarityColor(currentRewardItem.Rarity);
                itemDescriptionText.color = rarityColor;
            }
        }

        // 更新获取按钮状态
        private void UpdateClaimButton()
        {
            if (claimButton != null)
                // 奖励物品始终可以获取（免费）
                claimButton.interactable = true;
        }

        // 设置UI元素的激活状态
        private void SetUIElementsActive(bool active)
        {
            if (itemIconImage != null)
                itemIconImage.gameObject.SetActive(active);
            if (itemDescriptionText != null)
                itemDescriptionText.gameObject.SetActive(active);
            if (claimButton != null)
                claimButton.gameObject.SetActive(active);
        }

        // 获取按钮点击处理
        private void OnClaimButtonClicked()
        {
            if (currentRewardItem == null)
                return;

            // 触发获取事件
            OnClaimClicked?.Invoke(currentRewardItem);
        }

        // 直接获取奖励物品
        public bool ClaimRewardItem()
        {
            if (currentRewardItem == null)
                return false;

            // 触发获取事件
            OnClaimClicked?.Invoke(currentRewardItem);
            return true;
        }

        // 刷新显示
        public void RefreshDisplay()
        {
            UpdateDisplay();
        }

        // 获取当前奖励道具
        public RewardItemBase GetCurrentRewardItem()
        {
            return currentRewardItem;
        }

        // 清理显示
        public void ClearDisplay()
        {
            currentRewardItem = null;
            SetUIElementsActive(false);
        }

        // 设置获取按钮的可交互状态
        public void SetClaimButtonInteractable(bool interactable)
        {
            if (claimButton != null) claimButton.interactable = interactable;
        }

        // 获取奖励道具是否有效
        public bool HasValidRewardItem()
        {
            return currentRewardItem != null;
        }
    }
}