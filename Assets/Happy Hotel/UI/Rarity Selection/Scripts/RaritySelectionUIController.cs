using System;
using System.Collections.Generic;
using HappyHotel.Core.Rarity;
using HappyHotel.Shop;
using UnityEngine;
using UnityEngine.UI;

namespace HappyHotel.UI
{
    // 稀有度选择UI控制器
    // 负责显示弹出的选择界面，让玩家从3个同稀有度道具中选择1个
    public class RaritySelectionUIController : MonoBehaviour
    {
        [Header("UI面板")] [SerializeField] private GameObject selectionPanel; // 选择面板

        [Header("道具显示区域")] [SerializeField] private Transform itemContainer; // 道具容器

        [SerializeField] private RaritySelectionItemDisplayController itemDisplayPrefab; // 道具显示预制体

        [Header("按钮")] [SerializeField] private Button abandonButton; // 放弃按钮

        // 道具显示UI列表
        private readonly List<RaritySelectionItemDisplayController> itemDisplays = new();

        // 当前显示的道具列表
        private List<ShopItemBase> currentItems = new();

        // 当前稀有度
        private Rarity currentRarity;

        // 添加选择完成回调
        private Action<bool> onSelectionCompleted;

        private void Awake()
        {
            // 绑定按钮事件
            if (abandonButton != null) abandonButton.onClick.AddListener(OnAbandonClicked);

            // 默认隐藏面板
            HideSelectionUI();
        }

        private void OnDestroy()
        {
            // 清理事件绑定
            if (abandonButton != null) abandonButton.onClick.RemoveListener(OnAbandonClicked);
        }

        // 显示选择UI
        public void ShowSelectionUI(List<ShopItemBase> items, Rarity rarity,
            Action<bool> selectionCallback = null)
        {
            if (items == null || items.Count == 0)
            {
                Debug.LogWarning("没有可选择的道具，无法显示选择UI");
                return;
            }

            currentItems = new List<ShopItemBase>(items);
            currentRarity = rarity;
            onSelectionCompleted = selectionCallback;

            // 创建道具显示
            CreateItemDisplays();

            // 显示面板
            if (selectionPanel != null) selectionPanel.SetActive(true);

            Debug.Log($"显示稀有度选择UI，稀有度：{rarity}，道具数量：{items.Count}");
        }

        // 隐藏选择UI
        public void HideSelectionUI()
        {
            // 隐藏面板
            if (selectionPanel != null) selectionPanel.SetActive(false);

            // 清理道具显示
            ClearItemDisplays();

            // 清空当前数据
            currentItems.Clear();
            onSelectionCompleted = null;
        }

        // 创建道具显示UI
        private void CreateItemDisplays()
        {
            // 清理现有显示
            ClearItemDisplays();

            if (itemContainer == null || itemDisplayPrefab == null)
            {
                Debug.LogError("道具容器或预制体未设置");
                return;
            }

            // 为每个道具创建显示UI
            for (var i = 0; i < currentItems.Count && i < 3; i++)
            {
                var item = currentItems[i];
                CreateSingleItemDisplay(item, i);
            }
        }

        // 创建单个道具显示
        private void CreateSingleItemDisplay(ShopItemBase item, int index)
        {
            var displayController = Instantiate(itemDisplayPrefab, itemContainer);
            itemDisplays.Add(displayController);

            // 设置道具信息
            displayController.SetShopItem(item);

            // 绑定选择事件
            displayController.onItemSelected += OnItemSelected;
        }

        // 清理道具显示UI
        private void ClearItemDisplays()
        {
            foreach (var display in itemDisplays)
                if (display != null)
                {
                    // 清理事件绑定
                    display.onItemSelected -= OnItemSelected;
                    Destroy(display.gameObject);
                }

            itemDisplays.Clear();
        }

        // 处理道具选择事件
        private void OnItemSelected(ShopItemBase selectedItem)
        {
            if (selectedItem == null)
                return;

            Debug.Log($"玩家选择了道具：{selectedItem.ItemName}");

            // 执行道具的获取逻辑
            selectedItem.ExecutePurchaseLogic();

            // 先通知选择完成（选择了道具）
            Debug.Log("RaritySelectionUIController: 调用选择完成回调，itemSelected=true");
            onSelectionCompleted?.Invoke(true);

            // 然后关闭选择UI
            HideSelectionUI();
        }

        // 处理放弃按钮点击
        private void OnAbandonClicked()
        {
            Debug.Log("玩家选择放弃所有道具");

            // 先通知选择完成（放弃了选择）
            onSelectionCompleted?.Invoke(false);

            // 然后关闭选择UI
            HideSelectionUI();
        }

        // 获取稀有度名称
        private string GetRarityName(Rarity rarity)
        {
            return rarity switch
            {
                Rarity.Common => "普通",
                Rarity.Rare => "稀有",
                Rarity.Epic => "史诗",
                Rarity.Legendary => "传说",
                _ => "未知"
            };
        }

        // 获取稀有度颜色
        private Color GetRarityColor(Rarity rarity)
        {
            return RarityColorManager.GetRarityColor(rarity);
        }

        // 设置UI组件引用（用于动态配置）
        public void SetSelectionPanel(GameObject panel)
        {
            selectionPanel = panel;
        }

        public void SetItemContainer(Transform container)
        {
            itemContainer = container;
        }

        public void SetItemDisplayPrefab(RaritySelectionItemDisplayController prefab)
        {
            itemDisplayPrefab = prefab;
        }

        public void SetAbandonButton(Button button)
        {
            abandonButton = button;
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnAbandonClicked);
            }
        }
    }
}