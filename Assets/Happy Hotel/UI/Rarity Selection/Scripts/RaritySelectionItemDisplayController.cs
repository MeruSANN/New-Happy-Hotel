using System;
using HappyHotel.Core.Rarity;
using HappyHotel.Shop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HappyHotel.UI
{
    // 稀有度选择界面中单个道具显示控制器
    // 用于在选择界面中显示单个道具的信息和选择按钮
    public class RaritySelectionItemDisplayController : MonoBehaviour
    {
        [Header("UI组件")] [SerializeField] private Image itemIcon; // 道具图标

        [SerializeField] private TextMeshProUGUI itemNameText; // 道具名称
        [SerializeField] private TextMeshProUGUI itemDescriptionText; // 道具描述
        [SerializeField] private Button selectButton; // 选择按钮

        // 当前显示的道具
        private ShopItemBase currentItem;

        // 选择事件
        public Action<ShopItemBase> onItemSelected;

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

        // 设置要显示的道具
        public void SetShopItem(ShopItemBase item)
        {
            currentItem = item;
            UpdateDisplay();
        }

        // 更新显示内容
        private void UpdateDisplay()
        {
            if (currentItem == null)
            {
                ClearDisplay();
                return;
            }

            // 设置道具图标
            if (itemIcon != null)
            {
                itemIcon.sprite = currentItem.ItemIcon;
                itemIcon.gameObject.SetActive(currentItem.ItemIcon != null);
            }

            // 设置道具名称
            if (itemNameText != null) itemNameText.text = currentItem.ItemName;

            // 设置道具描述
            if (itemDescriptionText != null)
            {
                // 从物品中获取格式化描述
                var description = currentItem.GetFormattedDescription();

                // 如果没有描述，显示默认文本
                if (string.IsNullOrEmpty(description)) description = "无描述";

                itemDescriptionText.text = description;
            }

            // 启用选择按钮
            if (selectButton != null) selectButton.interactable = true;
        }

        // 清空显示内容
        private void ClearDisplay()
        {
            if (itemIcon != null)
            {
                itemIcon.sprite = null;
                itemIcon.gameObject.SetActive(false);
            }

            if (itemNameText != null) itemNameText.text = "";

            if (itemDescriptionText != null) itemDescriptionText.text = "";

            if (selectButton != null) selectButton.interactable = false;
        }

        // 处理选择按钮点击
        private void OnSelectButtonClicked()
        {
            if (currentItem != null) onItemSelected?.Invoke(currentItem);
        }

        // 获取稀有度颜色
        private Color GetRarityColor(Rarity rarity)
        {
            return RarityColorManager.GetRarityColor(rarity);
        }

        // 设置UI组件引用（用于动态配置）
        public void SetItemIcon(Image icon)
        {
            itemIcon = icon;
        }

        public void SetItemNameText(TextMeshProUGUI nameText)
        {
            itemNameText = nameText;
        }

        public void SetItemDescriptionText(TextMeshProUGUI descriptionText)
        {
            itemDescriptionText = descriptionText;
        }

        public void SetSelectButton(Button button)
        {
            // 清理旧的事件绑定
            if (selectButton != null) selectButton.onClick.RemoveListener(OnSelectButtonClicked);

            selectButton = button;

            // 绑定新的事件
            if (selectButton != null) selectButton.onClick.AddListener(OnSelectButtonClicked);
        }


        // 获取当前显示的道具
        public ShopItemBase GetCurrentItem()
        {
            return currentItem;
        }
    }
}