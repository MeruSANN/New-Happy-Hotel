using System;
using HappyHotel.Card;
using HappyHotel.UI.CardPileUI;
using UnityEngine;
using UnityEngine.UI;

namespace HappyHotel.UI.Hand
{
    // 单个卡牌槽位交互处理脚本
    public class CardInteractionHandler : MonoBehaviour
    {
        [Header("UI组件")] [SerializeField] private Button cardButton; // 槽位按钮

        [SerializeField] private CardItemDisplayer cardDisplayer; // 卡牌显示器
        [SerializeField] private Image highlightFrame; // 高亮外框图片

        // 当前槽位关联的卡牌实例
        private CardBase currentCard;
        private CardTypeId currentTypeId;

        // 槽位点击事件
        public Action<CardInteractionHandler> onSlotClicked;

        // 获取当前选中状态
        public bool IsSelected { get; private set; }

        // 获取当前临时状态
        public bool IsTemporary { get; private set; }

        // 检查槽位是否为空
        public bool IsEmpty => currentCard == null;

        private void Awake()
        {
            // 初始化按钮点击事件
            if (cardButton != null) cardButton.onClick.AddListener(OnSlotButtonClick);
            // 默认隐藏选中高亮
            if (highlightFrame != null) highlightFrame.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            // 清理事件监听
            if (cardButton != null) cardButton.onClick.RemoveListener(OnSlotButtonClick);
        }

        // 设置槽位关联的卡牌数据
        public void SetCard(CardBase card)
        {
            currentCard = card;
            currentTypeId = card?.TypeId;
            cardDisplayer?.DisplayCard(card);
            UpdateInteractable();
        }

        // 清空槽位关联的卡牌数据
        public void ClearSlot()
        {
            currentCard = null;
            currentTypeId = null;
            cardDisplayer?.DisplayCard(null);
            IsTemporary = false;
            UpdateInteractable();
        }

        // 设置选中状态
        public void SetSelected(bool selected)
        {
            IsSelected = selected;
            if (highlightFrame != null) highlightFrame.gameObject.SetActive(selected);
        }

        // 设置临时状态（新增）
        public void SetTemporaryState(bool isTemporary)
        {
            this.IsTemporary = isTemporary;

            if (cardDisplayer != null) cardDisplayer.SetTemporaryState(isTemporary);

            UpdateInteractable();
        }

        // 获取当前卡牌TypeId
        public CardTypeId GetCardTypeId()
        {
            return currentTypeId;
        }

        // 获取当前卡牌实例
        public CardBase GetCard()
        {
            return currentCard;
        }

        // 更新按钮可交互状态
        private void UpdateInteractable()
        {
            if (cardButton == null) return;

            // 只有非空且非临时状态的槽位才可以点击
            cardButton.interactable = !IsEmpty && !IsTemporary;
        }

        // 槽位按钮点击处理
        private void OnSlotButtonClick()
        {
            if (IsEmpty || IsTemporary) return;

            // 通知父级UI有槽位被点击
            onSlotClicked?.Invoke(this);
        }
    }
}