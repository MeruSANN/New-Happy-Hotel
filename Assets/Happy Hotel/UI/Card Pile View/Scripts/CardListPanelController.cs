using System.Collections.Generic;
using HappyHotel.Card;
using HappyHotel.Inventory;
using HappyHotel.Utils;
using UnityEngine;
using UnityEngine.UI;
using static HappyHotel.Inventory.CardInventory;

namespace HappyHotel.UI.CardPileUI
{
    /// <summary>
    ///     控制弹出式面板，显示指定区域的卡牌列表
    /// </summary>
    public class CardListPanelController : SingletonConnectedUIBase<CardInventory>
    {
        [Header("UI组件")] [SerializeField] private GameObject cardItemPrefab; // 应挂载 CardItemDisplayer 脚本

        [SerializeField] private Transform contentContainer; // ScrollView的Content
        [SerializeField] private Button closeButton; // 关闭按钮

        private readonly List<GameObject> instantiatedItems = new();
        private CardZone? currentDisplayingZone; // 记录当前正在显示的区域

        public bool IsVisible => gameObject.activeSelf;

        protected override void OnSingletonConnected()
        {
            SubscribeToEvents();
            SetupCloseButton();
            // 如果面板在连接成功时已经可见，则刷新一次
            RefreshIfVisible();
        }

        protected override void OnSingletonDisconnected()
        {
            UnsubscribeFromEvents();
        }

        private void SetupCloseButton()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(HidePanel);
            else
                Debug.LogWarning("CardListPanelController: 未找到关闭按钮引用，请检查Inspector中的设置");
        }

        private void SubscribeToEvents()
        {
            if (!IsConnectedToSingleton()) return;
            singletonInstance.onCardMovedBetweenZones += OnInventoryUpdated;
            singletonInstance.onCardAdded += OnInventoryUpdated;
            singletonInstance.onCardRemoved += OnInventoryUpdated;
        }

        private void UnsubscribeFromEvents()
        {
            if (!IsConnectedToSingleton()) return;
            singletonInstance.onCardMovedBetweenZones -= OnInventoryUpdated;
            singletonInstance.onCardAdded -= OnInventoryUpdated;
            singletonInstance.onCardRemoved -= OnInventoryUpdated;
        }

        private void OnInventoryUpdated(CardTypeId id, CardZone from, CardZone to)
        {
            RefreshIfVisible();
        }

        private void OnInventoryUpdated(CardTypeId id)
        {
            RefreshIfVisible();
        }

        private void RefreshIfVisible()
        {
            if (IsVisible && currentDisplayingZone.HasValue) ShowPanel(currentDisplayingZone.Value);
        }

        /// <summary>
        ///     显示指定区域的卡牌列表
        /// </summary>
        public void ShowPanel(CardZone zone)
        {
            // 如果尚未连接，则在显示时强制尝试连接一次
            if (!IsConnectedToSingleton())
            {
                ForceReconnect();
                // 如果尝试后仍然失败，则中止操作
                if (!IsConnectedToSingleton())
                {
                    Debug.LogWarning("CardListPanelController: 尝试连接后，仍未找到CardInventory，无法显示面板。");
                    return;
                }
            }

            currentDisplayingZone = zone;

            foreach (var item in instantiatedItems) Destroy(item);
            instantiatedItems.Clear();

            IEnumerable<CardBase> cards;
            if (zone == CardZone.All)
            {
                // 合并：牌库、弃牌、手牌、消耗区（排除临时区）
                var list = new List<CardBase>();
                list.AddRange(singletonInstance.GetCardsInZone(CardZone.Deck));
                list.AddRange(singletonInstance.GetCardsInZone(CardZone.Discard));
                list.AddRange(singletonInstance.GetCardsInZone(CardZone.Hand));
                list.AddRange(singletonInstance.GetCardsInZone(CardZone.Consumed));
                cards = list;
            }
            else
            {
                cards = singletonInstance.GetCardsInZone(zone);
            }

            foreach (var card in cards)
            {
                var cardItemGO = Instantiate(cardItemPrefab, contentContainer);
                var displayer = cardItemGO.GetComponent<CardItemDisplayer>();
                if (displayer != null)
                    displayer.DisplayCard(card);
                else
                    Debug.LogError($"卡牌预制体 {cardItemPrefab.name} 上缺少 CardItemDisplayer 脚本!");
                instantiatedItems.Add(cardItemGO);
            }

            if (!gameObject.activeSelf) gameObject.SetActive(true);
        }

        /// <summary>
        ///     隐藏面板
        /// </summary>
        public void HidePanel()
        {
            currentDisplayingZone = null;
            gameObject.SetActive(false);
        }
    }
}