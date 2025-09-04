using System;
using HappyHotel.Core.BehaviorComponent;
using HappyHotel.GameManager;
using UnityEngine;

namespace HappyHotel.Action.Components
{
    // 回合行动计数器组件，跟踪角色在当前回合执行的行动数量
    [DependsOnComponent(typeof(ActionQueueComponent))]
    public class TurnActionCounterComponent : BehaviorComponentBase, IEventListener
    {
        private int currentTurn;
        private int currentTurnActionCount;
        private ActionQueueComponent monitoredActionQueue;

        public override void OnAttach(BehaviorComponentContainer host)
        {
            base.OnAttach(host);

            // 获取ActionQueueComponent
            monitoredActionQueue = host.GetBehaviorComponent<ActionQueueComponent>();
            if (monitoredActionQueue != null) StartMonitoring();

            // 订阅玩家回合开始事件
            TurnManager.onPlayerTurnStart += OnTurnStart;

            // 初始化当前回合数
            if (TurnManager.Instance != null) currentTurn = TurnManager.Instance.GetCurrentTurn();
        }

        public override void OnDetach()
        {
            // 停止监听
            StopMonitoring();

            // 取消订阅回合事件
            TurnManager.onPlayerTurnStart -= OnTurnStart;

            base.OnDetach();
        }

        public void OnEvent(BehaviorComponentEvent evt)
        {
            if (evt.EventName == "SetActionQueue")
            {
                StopMonitoring();
                monitoredActionQueue = evt.Data as ActionQueueComponent;
                StartMonitoring();
            }
        }

        // 行动计数改变事件
        public event Action<int> onActionCountChanged;

        // 获取当前回合的行动数量
        public int GetCurrentTurnActionCount()
        {
            return currentTurnActionCount;
        }

        // 检查是否是回合的第一个行动
        public bool IsFirstActionOfTurn()
        {
            return currentTurnActionCount == 0;
        }

        // 开始监听ActionQueue
        private void StartMonitoring()
        {
            if (monitoredActionQueue != null)
            {
                monitoredActionQueue.onActionQueueChanged += OnActionQueueChanged;
                Debug.Log("TurnActionCounterComponent: 开始监听ActionQueue");
            }
        }

        // 停止监听ActionQueue
        private void StopMonitoring()
        {
            if (monitoredActionQueue != null)
            {
                monitoredActionQueue.onActionQueueChanged -= OnActionQueueChanged;
                monitoredActionQueue = null;
                Debug.Log("TurnActionCounterComponent: 停止监听ActionQueue");
            }
        }

        // 处理回合开始事件
        private void OnTurnStart(int turnNumber)
        {
            if (turnNumber != currentTurn)
            {
                // 新回合开始，重置计数器
                currentTurn = turnNumber;
                var oldCount = currentTurnActionCount;
                currentTurnActionCount = 0;

                Debug.Log($"TurnActionCounterComponent: 新回合开始 (回合 {turnNumber})，重置行动计数");

                // 触发事件
                if (oldCount != 0) onActionCountChanged?.Invoke(currentTurnActionCount);
            }
        }

        // 处理ActionQueue变化事件
        private void OnActionQueueChanged(ActionQueueEventArgs args)
        {
            if (args.EventType == ActionQueueEventType.ActionConsumed)
            {
                // 行动被消耗，增加计数
                currentTurnActionCount++;
                Debug.Log($"TurnActionCounterComponent: 行动被消耗，当前回合行动数: {currentTurnActionCount}");

                // 触发事件
                onActionCountChanged?.Invoke(currentTurnActionCount);
            }
        }
    }
}