using HappyHotel.Card;
using HappyHotel.Inventory;
using HappyHotel.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static HappyHotel.Inventory.CardInventory;

namespace HappyHotel.UI.CardPileUI
{
    // 主控制器，管理牌库、弃牌区、消耗区按钮的交互和数量显示
    public class CardPileViewController : SingletonConnectedUIBase<CardInventory>
    {
        [Header("区域按钮")] [SerializeField] private Button deckButton;

        [SerializeField] private Button discardButton;
        [SerializeField] private Button consumedButton;

        [Header("区域数量文本")] [SerializeField] private TMP_Text deckCountText;

        [SerializeField] private TMP_Text discardCountText;
        [SerializeField] private TMP_Text consumedCountText;

        [Header("卡牌列表面板")] [SerializeField] private CardListPanelController cardListPanel;

        private CardZone? _currentZone;

        protected override void OnUIStart()
        {
            // 添加按钮监听
            deckButton.onClick.AddListener(() => OnPileButtonClicked(CardZone.Deck));
            discardButton.onClick.AddListener(() => OnPileButtonClicked(CardZone.Discard));
            consumedButton.onClick.AddListener(() => OnPileButtonClicked(CardZone.Consumed));

            // 初始化时隐藏面板
            if (cardListPanel != null) cardListPanel.HidePanel();
        }

        protected override void OnSingletonConnected()
        {
            // 当成功连接到CardInventory单例后，订阅事件并立即刷新一次UI
            SubscribeToEvents();
            UpdateAllCounts();
        }

        protected override void OnSingletonDisconnected()
        {
            // 当与单例断开连接时，取消订阅事件
            UnsubscribeFromEvents();
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

        private void OnPileButtonClicked(CardZone zone)
        {
            if (cardListPanel == null) return;

            if (cardListPanel.IsVisible && _currentZone == zone)
            {
                cardListPanel.HidePanel();
                _currentZone = null;
            }
            else
            {
                cardListPanel.ShowPanel(zone);
                _currentZone = zone;
            }
        }

        private void OnInventoryUpdated(CardTypeId id, CardZone from, CardZone to)
        {
            UpdateAllCounts();
        }

        private void OnInventoryUpdated(CardTypeId id)
        {
            UpdateAllCounts();
        }

        private void UpdateAllCounts()
        {
            if (!IsConnectedToSingleton()) return;

            var deckCount = singletonInstance.DeckCardCount;
            var discardCount = singletonInstance.DiscardCardCount;
            var consumedCount = singletonInstance.ConsumedCardCount;

            deckCountText.text = deckCount.ToString();
            discardCountText.text = discardCount.ToString();
            consumedCountText.text = consumedCount.ToString();
        }
    }
}