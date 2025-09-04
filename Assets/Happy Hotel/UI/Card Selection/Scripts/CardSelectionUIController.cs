using System;
using System.Collections.Generic;
using HappyHotel.Core.Rarity;
using HappyHotel.Shop;
using UnityEngine;
using UnityEngine.UI;

namespace HappyHotel.UI
{
    // 卡牌选择UI控制器
    // 负责显示弹出的卡牌选择界面，让玩家从多个卡牌中选择1个
    public class CardSelectionUIController : MonoBehaviour
    {
        [Header("UI面板")] [SerializeField] private GameObject selectionPanel; // 选择面板

        [Header("卡牌显示区域")] [SerializeField] private Transform cardContainer; // 卡牌容器

        [SerializeField] private CardSelectionItemDisplayController cardDisplayPrefab; // 卡牌显示预制体

        [Header("按钮")] [SerializeField] private Button abandonButton; // 放弃按钮

        // 卡牌显示UI列表
        private readonly List<CardSelectionItemDisplayController> cardDisplays = new();

        // 当前显示的卡牌列表
        private List<CardShopItemBase> currentCards = new();

        // 当前稀有度（用于混合稀有度显示）
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

        // 显示卡牌选择UI（同稀有度）
        public void ShowCardSelectionUI(List<CardShopItemBase> cards, Rarity rarity,
            Action<bool> selectionCallback = null)
        {
            if (cards == null || cards.Count == 0)
            {
                Debug.LogWarning("没有可选择的卡牌，无法显示选择UI");
                return;
            }

            currentCards = new List<CardShopItemBase>(cards);
            currentRarity = rarity;
            onSelectionCompleted = selectionCallback;

            // 创建卡牌显示
            CreateCardDisplays();

            // 显示面板
            if (selectionPanel != null) selectionPanel.SetActive(true);

            Debug.Log($"显示卡牌选择UI，稀有度：{rarity}，卡牌数量：{cards.Count}");
        }

        // 显示卡牌选择UI（混合稀有度）
        public void ShowMixedRarityCardSelectionUI(List<CardShopItemBase> cards,
            Action<bool> selectionCallback = null)
        {
            ShowCardSelectionUI(cards, Rarity.Common, selectionCallback);
        }

        // 隐藏选择UI
        public void HideSelectionUI()
        {
            // 隐藏面板
            if (selectionPanel != null) selectionPanel.SetActive(false);

            // 清理卡牌显示
            ClearCardDisplays();

            // 清空当前数据
            currentCards.Clear();
            onSelectionCompleted = null;
        }

        // 创建卡牌显示UI
        private void CreateCardDisplays()
        {
            // 清理现有显示
            ClearCardDisplays();

            if (cardContainer == null || cardDisplayPrefab == null)
            {
                Debug.LogError("卡牌容器或预制体未设置");
                return;
            }

            // 为每个卡牌创建显示UI
            for (var i = 0; i < currentCards.Count && i < 3; i++)
            {
                var card = currentCards[i];
                CreateSingleCardDisplay(card, i);
            }
        }

        // 创建单个卡牌显示
        private void CreateSingleCardDisplay(CardShopItemBase card, int index)
        {
            var displayController = Instantiate(cardDisplayPrefab, cardContainer);
            cardDisplays.Add(displayController);

            // 设置卡牌信息
            displayController.SetCard(card);

            // 绑定选择事件
            displayController.onItemSelected += OnCardSelected;
        }

        // 清理卡牌显示UI
        private void ClearCardDisplays()
        {
            foreach (var display in cardDisplays)
                if (display != null)
                {
                    // 清理事件绑定
                    display.onItemSelected -= OnCardSelected;
                    Destroy(display.gameObject);
                }

            cardDisplays.Clear();
        }

        // 处理卡牌选择事件
        private void OnCardSelected(ShopItemBase selectedCard)
        {
            if (selectedCard == null)
                return;

            Debug.Log($"玩家选择了卡牌：{selectedCard.ItemName}");

            // 执行卡牌的获取逻辑
            selectedCard.ExecutePurchaseLogic();

            // 先通知选择完成（选择了道具）
            Debug.Log("CardSelectionUIController: 调用选择完成回调，itemSelected=true");
            onSelectionCompleted?.Invoke(true);

            // 然后关闭选择UI
            HideSelectionUI();
        }

        // 处理放弃按钮点击
        private void OnAbandonClicked()
        {
            Debug.Log("玩家选择放弃所有卡牌");

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

        public void SetCardContainer(Transform container)
        {
            cardContainer = container;
        }

        public void SetCardDisplayPrefab(CardSelectionItemDisplayController prefab)
        {
            cardDisplayPrefab = prefab;
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