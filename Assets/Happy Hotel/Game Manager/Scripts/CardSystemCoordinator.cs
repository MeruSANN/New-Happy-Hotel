using HappyHotel.Card;
using HappyHotel.Core.Singleton;
using HappyHotel.Inventory;
using HappyHotel.UI.Hand;
using UnityEngine;

namespace HappyHotel.GameManager
{
    // 卡牌系统协调器
    // 职责：协调UI和放置控制器的交互，实现松耦合
    [ManagedSingleton(SceneLoadMode.Include, false, "GameScene")]
    [SingletonInitializationDependency(typeof(CardUseController))]
    public class CardSystemCoordinator : SingletonBase<CardSystemCoordinator>
    {
        // 组件引用
        private HandUI inventoryUI;
        private CardUseController useController;

        private void Update()
        {
            HandleGlobalInput();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            UnsubscribeFromEvents();
        }

        protected override void OnSingletonAwake()
        {
            InitializeComponents();
            SubscribeToEvents();
        }

        #region 全局输入处理

        // 处理全局输入
        private void HandleGlobalInput()
        {
            // 右键取消选中功能已移除，改为只有在道具为0时才取消选中
            // 这样可以避免用户在放置过程中意外取消选中
        }

        #endregion

        #region 初始化

        // 初始化组件引用
        private void InitializeComponents()
        {
            inventoryUI = FindObjectOfType<HandUI>();
            useController = CardUseController.Instance;

            if (!inventoryUI)
                Debug.LogWarning("未找到CardInventoryUI组件");

            if (!useController)
                Debug.LogWarning("未找到ActiveCardUseController组件");
        }

        // 订阅事件
        private void SubscribeToEvents()
        {
            if (inventoryUI != null)
            {
                inventoryUI.onCardSelected += OnCardSelected;
                inventoryUI.onSelectionCleared += OnSelectionCleared;
            }

            if (useController != null)
            {
                useController.onCardUsed += OnCardUsed;
                useController.onCardRestored += OnCardRestored;
                useController.onStateChanged += OnUseStateChanged;
            }
        }

        // 取消订阅事件
        private void UnsubscribeFromEvents()
        {
            if (inventoryUI != null)
            {
                inventoryUI.onCardSelected -= OnCardSelected;
                inventoryUI.onSelectionCleared -= OnSelectionCleared;
            }

            if (useController != null)
            {
                useController.onCardUsed -= OnCardUsed;
                useController.onCardRestored -= OnCardRestored;
                useController.onStateChanged -= OnUseStateChanged;
            }
        }

        #endregion

        #region 事件处理

        // 卡牌被选中
        private void OnCardSelected(CardBase card)
        {
            if (useController != null) useController.StartUse(card);
        }

        // 选中状态被清除
        private void OnSelectionCleared()
        {
            if (useController != null) useController.CancelUse();
        }

        // 卡牌被使用（放置成功）
        private void OnCardUsed(CardTypeId typeId)
        {
            // 注意：CardBase.UseCard()已经通过HandleConsumableLogic()处理了消耗逻辑
            // 这里只需要处理UI相关的逻辑，不需要再次调用CardDrawManager

            // 使用卡牌后取消选中状态
            if (inventoryUI != null) inventoryUI.ClearSelection();
        }

        // 卡牌被恢复（删除已放置的道具）
        private void OnCardRestored(CardTypeId typeId)
        {
            // 删除道具后应回到Idle，不自动选中任何卡牌
            if (inventoryUI != null) inventoryUI.ClearSelection();
            if (useController != null) useController.CancelUse();
        }

        // 放置状态改变
        private void OnUseStateChanged(CardUseController.UseState newState)
        {
            // 可以在这里处理UI反馈，比如光标变化、提示文本等
            switch (newState)
            {
                case CardUseController.UseState.Idle:
                    // 进入空闲状态，可以显示默认光标
                    break;
                case CardUseController.UseState.SelectingPosition:
                    // 进入位置选择状态，可以显示放置光标
                    break;
                case CardUseController.UseState.SelectingDirection:
                    // 进入方向选择状态，可以显示方向选择光标
                    break;
                case CardUseController.UseState.SelectingTarget:
                    // 进入目标选择状态，可以显示目标选择光标
                    break;
            }
        }

        #endregion

        #region 公共接口

        // 强制刷新系统状态（用于外部调用）
        public void RefreshSystemState()
        {
            if (inventoryUI != null) inventoryUI.RefreshDisplay();
        }

        // 获取当前放置状态
        public CardUseController.UseState GetUseState()
        {
            return useController?.GetCurrentState() ?? CardUseController.UseState.Idle;
        }

        // 检查是否正在使用
        public bool IsUsing()
        {
            return useController?.IsUsing ?? false;
        }

        public void RemoveCardFromInventory(CardTypeId typeId)
        {
            // 使用新的CardDrawManager来处理卡牌移除
            if (CardDrawManager.Instance != null)
            {
                CardDrawManager.Instance.UseCard(typeId);
            }
            else
            {
                // 回退到旧的方式
                if (CardInventory.Instance != null) CardInventory.Instance.RemoveCard(typeId);
            }
        }

        public bool HasCardOfType(CardTypeId typeId)
        {
            // 优先检查手牌区
            if (CardInventory.Instance != null)
                return CardInventory.Instance.HasCardOfType(typeId, CardInventory.CardZone.Hand);
            return false;
        }

        #endregion
    }
}