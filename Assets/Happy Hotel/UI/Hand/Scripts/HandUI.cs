using System;
using System.Collections.Generic;
using System.Linq;
using HappyHotel.Card;
using HappyHotel.Inventory;
using HappyHotel.Utils;
using UnityEngine;

namespace HappyHotel.UI.Hand
{
    // 负责手牌UI显示和交互
    public class HandUI : SingletonConnectedUIBase<CardInventory>
    {
        [Header("UI组件")] [SerializeField] private Transform cardContainer;

        [SerializeField] private GameObject cardPrefab;

        // 卡牌UI列表，与手牌一一对应
        private readonly List<CardInteractionHandler> cardUIs = new();

        // 添加标志来避免在卡牌使用过程中触发选中事件
        private bool isProcessingCardUse;

        // 事件：当卡牌被选中或选择被清除时触发
        public Action<CardBase> onCardSelected;
        public System.Action onSelectionCleared;

        // 当前选中的卡牌UI索引
        private int selectedCardIndex = -1;

        #region 基类方法重写

        // 当单例连接成功时调用
        protected override void OnSingletonConnected()
        {
            SubscribeToInventoryEvents();
            RefreshDisplay();
        }

        // 当单例断开连接时调用
        protected override void OnSingletonDisconnected()
        {
            UnsubscribeFromInventoryEvents();
        }

        #endregion

        #region 公共接口

        // 刷新整个手牌显示
        public void RefreshDisplay()
        {
            if (!IsConnectedToSingleton()) return;

            var handCards = GetHandCardList();

            UpdateSlotCount(handCards.Count);
            UpdateSlotData(handCards);
            RestoreSelectionIfValid();
        }

        // 根据卡牌类型ID选中一个UI槽位
        public void SelectSlotByType(CardTypeId typeId)
        {
            if (typeId == null) return;

            for (var i = 0; i < cardUIs.Count; i++)
                if (cardUIs[i].GetCardTypeId()?.Equals(typeId) == true)
                {
                    SelectSlot(i);
                    return;
                }
        }

        // 清除选中状态
        public void ClearSelection()
        {
            if (selectedCardIndex >= 0 && selectedCardIndex < cardUIs.Count)
                cardUIs[selectedCardIndex].SetSelected(false);

            if (selectedCardIndex != -1)
            {
                selectedCardIndex = -1;
                onSelectionCleared?.Invoke();
                Debug.Log("卡牌选择已清除");
            }
        }

        // 获取当前选中的卡牌实例
        public CardBase GetSelectedCard()
        {
            return selectedCardIndex >= 0 && selectedCardIndex < cardUIs.Count
                ? cardUIs[selectedCardIndex].GetCard()
                : null;
        }

        // 检查是否有卡牌被选中
        public bool HasSelection => selectedCardIndex >= 0;

        #endregion

        #region 私有方法

        // 从背包获取手牌列表
        private IReadOnlyList<CardBase> GetHandCardList()
        {
            // 只获取手牌区的卡牌
            return singletonInstance.GetHandCards();
        }

        // 根据手牌数量调整UI槽位的数量
        private void UpdateSlotCount(int requiredCount)
        {
            while (cardUIs.Count < requiredCount) CreateSlot();

            while (cardUIs.Count > requiredCount) RemoveLastSlot();
        }

        // 将手牌数据填充到UI槽位中
        private void UpdateSlotData(IReadOnlyList<CardBase> handCards)
        {
            var temporaryCards = singletonInstance.GetTemporaryCards();

            for (var i = 0; i < handCards.Count && i < cardUIs.Count; i++)
            {
                var card = handCards[i];
                cardUIs[i].SetCard(card);

                // 检查卡牌是否在临时区，设置相应的状态
                // 通过比较卡牌实例来判断，确保精确匹配
                var isTemporary = temporaryCards.Any(tempCard => ReferenceEquals(tempCard, card));
                cardUIs[i].SetTemporaryState(isTemporary);
            }
        }

        // 在刷新后尝试恢复之前的选中状态
        private void RestoreSelectionIfValid()
        {
            if (selectedCardIndex >= 0 && selectedCardIndex < cardUIs.Count && !cardUIs[selectedCardIndex].IsEmpty)
            {
                cardUIs[selectedCardIndex].SetSelected(true);

                // 只有在非卡牌使用过程中才触发选中事件
                if (!isProcessingCardUse)
                {
                    var selectedCard = cardUIs[selectedCardIndex].GetCard();
                    onCardSelected?.Invoke(selectedCard);
                }
            }
            else
            {
                // 如果之前的选中项变得无效（例如卡牌被用掉），则清除选中
                ClearSelection();
            }
        }

        // 创建一个新的UI槽位
        private void CreateSlot()
        {
            if (cardContainer == null || cardPrefab == null) return;

            var slotObj = Instantiate(cardPrefab, cardContainer);
            var slotUI = slotObj.GetComponent<CardInteractionHandler>();

            if (slotUI != null)
            {
                var slotIndex = cardUIs.Count;
                slotUI.onSlotClicked += clickedSlot => OnSlotClicked(slotIndex);
                cardUIs.Add(slotUI);
            }
        }

        // 移除最后一个UI槽位
        private void RemoveLastSlot()
        {
            if (cardUIs.Count == 0) return;

            var lastIndex = cardUIs.Count - 1;
            var lastSlot = cardUIs[lastIndex];

            if (selectedCardIndex == lastIndex) selectedCardIndex = -1; // 在RestoreSelectionIfValid中会重新选择

            cardUIs.RemoveAt(lastIndex);
            if (lastSlot != null)
                // 使用Destroy而不是DestroyImmediate，以避免在某些Unity回调中出现问题
                Destroy(lastSlot.gameObject);
        }

        // UI槽位点击事件处理
        private void OnSlotClicked(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= cardUIs.Count || cardUIs[slotIndex].IsEmpty) return;

            if (selectedCardIndex == slotIndex)
            {
                // 重新触发选中事件，确保外部系统状态同步
                var selectedCard = cardUIs[slotIndex].GetCard();
                onCardSelected?.Invoke(selectedCard);
                return;
            }

            SelectSlot(slotIndex);
        }

        // 选中指定的UI槽位
        private void SelectSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= cardUIs.Count) return;

            // 取消上一个选中
            if (selectedCardIndex >= 0 && selectedCardIndex < cardUIs.Count)
                cardUIs[selectedCardIndex].SetSelected(false);

            // 设置新选中
            selectedCardIndex = slotIndex;
            cardUIs[slotIndex].SetSelected(true);

            // 触发选中事件
            var selectedCard = cardUIs[slotIndex].GetCard();
            onCardSelected?.Invoke(selectedCard);
        }

        // 订阅背包事件
        private void SubscribeToInventoryEvents()
        {
            if (IsConnectedToSingleton())
                // 监听卡牌在区域间的移动，这是手牌变化最主要的来源
                singletonInstance.onCardMovedBetweenZones += OnCardMoved;
        }

        // 取消订阅背包事件
        private void UnsubscribeFromInventoryEvents()
        {
            if (IsConnectedToSingleton()) singletonInstance.onCardMovedBetweenZones -= OnCardMoved;
        }

        // 卡牌区域移动事件处理
        private void OnCardMoved(CardTypeId cardTypeId, CardInventory.CardZone fromZone, CardInventory.CardZone toZone)
        {
            // 关心进入或离开手牌区的变化，以及临时区的变化
            if (fromZone == CardInventory.CardZone.Hand || toZone == CardInventory.CardZone.Hand ||
                fromZone == CardInventory.CardZone.Temporary || toZone == CardInventory.CardZone.Temporary)
            {
                // 设置卡牌使用处理标志，避免在刷新过程中触发选中事件
                isProcessingCardUse = true;
                RefreshDisplay();
                isProcessingCardUse = false;
            }
        }

        #endregion
    }
}