using System;
using HappyHotel.Card;
using HappyHotel.Core;
using HappyHotel.Core.Grid;
using HappyHotel.Core.Singleton;
using HappyHotel.Inventory;
using HappyHotel.Map;
using HappyHotel.Prop;
using HappyHotel.Prop.Settings;
using HappyHotel.UI;
using HappyHotel.UI.Hand;
using HappyHotel.Utils;
using UnityEngine;
// 

namespace HappyHotel.GameManager
{
    // 优化后的卡牌使用控制器
    // 职责：专注于使用流程控制，减少与UI的直接耦合
    [ManagedSingleton(SceneLoadMode.Include, false, "GameScene")]
    [SingletonInitializationDependency(typeof(GridObjectManager))]
    public class CardUseController : SingletonBase<CardUseController>
    {
        // 使用状态枚举
        public enum UseState
        {
            Idle,
            SelectingPosition,
            SelectingDirection,
            SelectingTarget
        }

        private CardBase currentCard;

        // 当前状态
        private UseState currentState = UseState.Idle;
        private DirectionSelectorUI directionSelector;

        // 依赖组件
        private GridObjectManager gridManager;
        private HandUI handUI; // 对HandUI的引用

        // 敏捷相关逻辑已移除

        // 添加使用状态标志，防止重复处理
        private bool isProcessingCardUse;
        public Action<CardTypeId> onCardRestored;

        // 使用相关事件
        public Action<CardTypeId> onCardUsed;
        public Action<UseState> onStateChanged;
        private Vector2Int selectedPosition;
        private TargetGridIndicator targetIndicator; // 目标格子指示器

        private void OnEnable()
        {
            TurnManager.onPlayerTurnEnd += HandlePhaseLock;
            TurnManager.onEnemyTurnStart += HandlePhaseLock;
        }

        private void OnDisable()
        {
            TurnManager.onPlayerTurnEnd -= HandlePhaseLock;
            TurnManager.onEnemyTurnStart -= HandlePhaseLock;
        }

        private void Update()
        {
            // 只有在游戏静止且玩家阶段时才处理输入
            if (!IsGameStateValid())
            {
                if (currentState != UseState.Idle) CancelUse();
                return;
            }

            HandleInput();
        }

        protected override void OnSingletonAwake()
        {
            InitializeDependencies();
        }

        // 离开玩家阶段时立刻取消用牌并清除手牌选中
        private void HandlePhaseLock(int turn)
        {
            if (currentState != UseState.Idle) CancelUse();
            if (handUI != null && handUI.HasSelection) handUI.ClearSelection();
        }

        #region 私有方法 - 指示器管理

        private void UpdateIndicatorForState()
        {
            if (targetIndicator == null) return;

            if (currentState == UseState.SelectingPosition || currentState == UseState.SelectingTarget)
                targetIndicator.Activate();
            else
                targetIndicator.Deactivate();
        }

        #endregion

        #region 公共接口

        // 开始使用流程
        public void StartUse(CardBase card)
        {
            // 若正在处理或非空闲，先取消当前流程以便切换
            if (isProcessingCardUse || currentState != UseState.Idle) CancelUse();

            if (card == null || !IsGameStateValid())
            {
                Debug.LogWarning("[CardUseController] StartUse: 无法开始使用：参数无效或游戏状态不正确");
                return;
            }

            // 检查费用是否足够
            if (CostManager.Instance != null)
            {
                var cost = card.Template.cardCost;
                if (!CostManager.Instance.HasEnoughCost(cost))
                {
                    Debug.LogWarning(
                        $"[CardUseController] StartUse: 费用不足，无法使用卡牌 {card.TypeId}。需要: {cost}, 当前: {CostManager.Instance.CurrentCost}");
                    // 在这里可以触发一个UI提示
                    return;
                }
            }

            // 设置使用状态标志
            isProcessingCardUse = true;
            currentCard = card;

            // 根据卡牌类型设置不同的状态
            if (card is TargetSelectionCard)
                SetState(UseState.SelectingTarget);
            else if (card is ActivePlacementCard)
                SetState(UseState.SelectingPosition);
            else
                // 普通卡牌直接使用
                UseCardDirectly();
        }

        // 取消使用
        public void CancelUse()
        {
            if (currentState == UseState.SelectingDirection) HideDirectionSelector();

            ResetState();
        }

        // 获取当前状态
        public UseState GetCurrentState()
        {
            return currentState;
        }

        // 检查是否正在使用
        public bool IsUsing => currentState != UseState.Idle;

        #endregion

        #region 私有方法 - 状态管理

        // 设置状态（统一状态管理）
        private void SetState(UseState newState)
        {
            if (currentState != newState)
            {
                currentState = newState;
                UpdateIndicatorForState();
                onStateChanged?.Invoke(currentState);
            }
        }

        // 重置状态
        private void ResetState()
        {
            currentCard = null;
            selectedPosition = Vector2Int.zero;
            isProcessingCardUse = false; // 重置使用状态标志
            SetState(UseState.Idle);
        }

        #endregion

        #region 私有方法 - 输入处理

        // 处理输入（简化逻辑）
        private void HandleInput()
        {
            // 响应右键点击，用于取消卡牌选择或删除场上道具
            if (Input.GetMouseButtonDown(1))
            {
                // 先处理取消选择
                if (handUI != null && handUI.HasSelection)
                {
                    handUI.ClearSelection();
                    CancelUse(); // 如果正在使用卡牌，也一并取消
                }

                // 然后处理删除场上道具（空闲状态下）
                if (currentState == UseState.Idle) HandleRightClickDelete();
            }

            switch (currentState)
            {
                case UseState.Idle:
                    HandleIdleInput();
                    break;
                case UseState.SelectingPosition:
                    HandlePositionSelection();
                    break;
                case UseState.SelectingDirection:
                    HandleDirectionSelection();
                    break;
                case UseState.SelectingTarget:
                    HandleTargetSelection();
                    break;
            }
        }

        // 处理空闲状态输入
        private void HandleIdleInput()
        {
            // 空闲状态下无额外逻辑
        }

        // 处理右键删除场上道具（新增）
        private void HandleRightClickDelete()
        {
            var mousePos = GetMouseGridPosition();
            var prop = GetPropAtPosition(mousePos);

            if (prop != null) DeletePropAndRestoreCard(prop, mousePos);
        }

        // 删除道具并恢复卡牌（新增）
        private void DeletePropAndRestoreCard(PropBase prop, Vector2Int position)
        {
            var placedCard = prop.GetPlacedByCard();

            // 只有当对象由卡牌放置时才允许删除
            if (placedCard == null)
            {
                Debug.Log("目标对象未由卡牌放置，不能删除");
                return;
            }

            // 从临时区移除卡牌（不从手牌区移除）
            if (CardInventory.Instance != null) CardInventory.Instance.RemoveFromTemporaryZone(placedCard);

            // 对于放置型卡牌，恢复费用
            if (placedCard is ActivePlacementCard && CostManager.Instance != null)
                CostManager.Instance.AddCost(placedCard.Template.cardCost);

            // 通知卡牌被恢复
            onCardRestored?.Invoke(placedCard.TypeId);

            Debug.Log($"已删除道具并恢复卡牌 {placedCard.TypeId.Id}");

            // 删除道具
            PropController.Instance?.RemoveProp(position);
        }

        // 敏捷放置相关逻辑已删除

        // 处理位置选择
        private void HandlePositionSelection()
        {
            var mousePosForIndicator = GetMouseGridPosition();
            targetIndicator?.SetValid(IsValidUsePosition(mousePosForIndicator));

            if (Input.GetMouseButtonDown(0)) HandleLeftClickForPosition();
            // 右键取消逻辑已移至顶层，这里不再需要
        }

        // 处理方向选择
        private void HandleDirectionSelection()
        {
            // 右键取消逻辑已移至顶层，这里不再需要
        }

        // 处理目标选择
        private void HandleTargetSelection()
        {
            var mousePosForIndicator = GetMouseGridPosition();
            var targetProp = GetPropAtPosition(mousePosForIndicator);

            var isValidTarget = IsTargetValidForCurrentCard(targetProp);
            targetIndicator?.SetValid(isValidTarget);

            if (Input.GetMouseButtonDown(0))
            {
                if (isValidTarget)
                {
                    HandleLeftClickForTarget();
                }
                // 不合法则拦截点击，不进入下一步
            }
        }

        // 校验当前选中卡牌的目标是否合法
        private bool IsTargetValidForCurrentCard(PropBase target)
        {
            if (currentCard is TargetSelectionCard targetCard)
                return targetCard.IsValidTarget(target);
            return false;
        }

        // 左键：位置选择
        private void HandleLeftClickForPosition()
        {
            var mousePos = GetMouseGridPosition();
            if (IsValidUsePosition(mousePos))
            {
                selectedPosition = mousePos;

                if (currentCard != null && currentCard is DirectionalPlacementCard)
                    ShowDirectionSelector(mousePos);
                else
                    UseCard(mousePos, null);
            }
        }

        // 左键：目标选择
        private void HandleLeftClickForTarget()
        {
            var mousePos = GetMouseGridPosition();
            var targetProp = GetPropAtPosition(mousePos);

            if (targetProp != null && currentCard is TargetSelectionCard targetCard)
                UseTargetCard(targetCard, targetProp);
        }

        #endregion

        #region 私有方法 - 使用逻辑

        // 统一的使用逻辑（合并重复代码）
        private void UseCard(Vector2Int position, Direction? direction)
        {
            if (currentCard == null)
            {
                Debug.LogError("当前没有要使用的卡牌");
                CancelUse();
                return;
            }

            // 创建设置
            IPropSetting setting = direction.HasValue ? new DirectionalSetting(direction.Value) : null;

            // 统一使用UseCard方法
            var success = false;
            if (currentCard is ActivePlacementCard placementCard)
                // 对于放置卡牌，使用带位置参数的UseCard方法
                success = placementCard.UseCard(position, setting);
            else
                // 对于普通卡牌，使用无参数的UseCard方法
                success = currentCard.UseCard();

            if (success)
                HandleSuccessfulUse();
            else
                HandleFailedUse();
        }

        // 使用目标选择卡牌
        private void UseTargetCard(TargetSelectionCard targetCard, PropBase targetProp)
        {
            if (targetCard == null || targetProp == null)
            {
                Debug.LogError("目标卡牌或目标Prop为空");
                CancelUse();
                return;
            }

            var success = targetCard.UseCard(targetProp);

            if (success)
                HandleSuccessfulUse();
            else
                HandleFailedUse();
        }

        // 直接使用卡牌（不需要选择位置或目标）
        private void UseCardDirectly()
        {
            if (currentCard == null)
            {
                Debug.LogError("[CardUseController] UseCardDirectly: 当前没有要使用的卡牌");
                CancelUse();
                return;
            }

            var success = currentCard.UseCard();

            // 再次检查currentCard是否为空，防止在UseCard过程中被其他流程清空
            if (currentCard == null)
            {
                Debug.LogError("[CardUseController] UseCardDirectly: 卡牌使用后currentCard变为空，可能是重复处理导致的");
                CancelUse();
                return;
            }

            if (success)
                HandleSuccessfulUse();
            else
                HandleFailedUse();
        }

        // 处理成功使用
        private void HandleSuccessfulUse()
        {
            if (currentCard == null)
            {
                Debug.LogError("[CardUseController] HandleSuccessfulUse: currentCard 为空！");
                return;
            }

            var typeId = currentCard.TypeId;

            // 扣除费用
            if (CostManager.Instance != null) CostManager.Instance.UseCost(currentCard.Template.cardCost);

            // 通知卡牌被使用
            onCardUsed?.Invoke(typeId);

            // 隐藏方向选择器
            HideDirectionSelector();

            // 直接重置状态
            ResetState();
        }

        // 处理失败使用
        private void HandleFailedUse()
        {
            Debug.LogError("使用卡牌失败");
            HideDirectionSelector();
            ResetState();
        }

        #endregion

        #region 私有方法 - 方向选择

        // 显示方向选择器
        private void ShowDirectionSelector(Vector2Int position)
        {
            if (!directionSelector)
            {
                Debug.LogError("DirectionSelectorUI未找到");
                CancelUse();
                return;
            }

            SetState(UseState.SelectingDirection);

            var worldPos = GridToWorldPosition(position);
            if (Camera.main != null)
            {
                var screenPos = Camera.main.WorldToScreenPoint(worldPos);

                directionSelector.Show(screenPos, OnDirectionSelected);

                // 根据卡牌设置配置方向按钮的可用状态
                ConfigureDirectionButtons();
            }
        }

        // 隐藏方向选择器
        private void HideDirectionSelector()
        {
            directionSelector?.Hide();
        }

        // 方向选择回调
        private void OnDirectionSelected(Direction direction)
        {
            UseCard(selectedPosition, direction);
        }

        // 敏捷方向选择相关逻辑已删除

        // 配置方向按钮的可用状态
        private void ConfigureDirectionButtons()
        {
            if (currentCard is DirectionalPlacementCard directionalCard && directionSelector != null)
            {
                // 重置所有按钮状态
                directionSelector.ResetButtonStates();

                // 根据卡牌设置禁用不允许的方向按钮
                var allowedDirections = directionalCard.GetAllowedDirections();
                var allDirections = new[] { Direction.Up, Direction.Down, Direction.Left, Direction.Right };

                foreach (var direction in allDirections)
                {
                    var isAllowed = directionalCard.IsDirectionAllowed(direction);
                    directionSelector.SetDirectionButtonEnabled(direction, isAllowed);
                }
            }
        }

        #endregion

        #region 私有方法 - 工具方法

        // 初始化依赖组件
        private void InitializeDependencies()
        {
            gridManager = FindObjectOfType<GridObjectManager>();
            directionSelector = FindObjectOfType<DirectionSelectorUI>(true);
            handUI = FindObjectOfType<HandUI>(); // 查找HandUI实例
            targetIndicator = FindObjectOfType<TargetGridIndicator>(true);

            if (gridManager == null)
                Debug.LogWarning("未找到GridObjectManager");
            if (directionSelector == null)
                Debug.LogWarning("未找到DirectionSelectorUI");
            if (handUI == null)
                Debug.LogWarning("未找到HandUI");
            if (targetIndicator == null)
                Debug.LogWarning("未找到TargetGridIndicator");
        }

        // 检查游戏状态是否有效（必须 Idle 且为玩家阶段）
        private bool IsGameStateValid()
        {
            if (GameManager.Instance?.GetGameState() != GameManager.GameState.Idle) return false;
            var tm = TurnManager.Instance;
            if (tm == null) return false;
            return tm.GetCurrentPhase() == TurnManager.TurnPhase.Player;
        }

        // 获取鼠标网格位置
        private Vector2Int GetMouseGridPosition()
        {
            var mouseScreenPos = Input.mousePosition;

            if (Camera.main)
            {
                var mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
                mouseWorldPos.z = 0;

                Vector2Int gridPos;
                if (gridManager != null)
                    gridPos = gridManager.WorldToGrid(mouseWorldPos);
                else
                    gridPos = new Vector2Int(Mathf.RoundToInt(mouseWorldPos.x), Mathf.RoundToInt(mouseWorldPos.y));

                return gridPos;
            }

            Debug.LogWarning("[CardUseController] Camera.main为空");

            return default;
        }

        // 网格位置转世界位置
        private Vector3 GridToWorldPosition(Vector2Int gridPos)
        {
            return gridManager ? gridManager.GridToWorld(gridPos) : new Vector3(gridPos.x, gridPos.y, 0);
        }

        // 检查位置是否可以使用
        private bool IsValidUsePosition(Vector2Int position)
        {
            var mapManager = MapManager.Instance;
            var gridObjectManager = GridObjectManager.Instance;

            if (!mapManager || !gridObjectManager) return false;

            // 必须是地板且没有其他对象
            return mapManager.IsFloor(position.x, position.y) &&
                   gridObjectManager.GetObjectsAt(position).Count == 0;
        }

        // 检查手牌区是否拥有某类型卡牌
        private bool HasCardOfType(CardTypeId typeId)
        {
            return CardInventory.Instance?.HasCardOfType(typeId, CardInventory.CardZone.Hand) == true;
        }

        // 获取指定位置的Prop
        private PropBase GetPropAtPosition(Vector2Int position)
        {
            if (gridManager == null) return null;

            var objects = gridManager.GetObjectsAt(position);
            foreach (var obj in objects)
                if (obj is PropBase prop)
                    return prop;

            return null;
        }

        // 添加卡牌到背包
        private void AddCardToInventory(CardTypeId cardTypeId)
        {
            CardInventory.Instance?.AddCard(cardTypeId);
        }

        #endregion
    }
}