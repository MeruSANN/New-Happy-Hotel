using System;
using System.Collections.Generic;
using System.Linq;
using HappyHotel.Core.BehaviorComponent;
using UnityEngine;

namespace HappyHotel.Action.Components
{
    public class ActionQueueComponent : BehaviorComponentBase
    {
        // 使用List代替Queue以支持任意位置移除
        private readonly List<IAction> actionQueue = new();

        private int maxQueueSize = 10;

        public event Action<ActionQueueEventArgs> onActionQueueChanged;

        public bool AddAction(IAction action)
        {
            if (actionQueue.Count < maxQueueSize)
            {
                actionQueue.Add(action); // 使用List.Add
                action.SetActionQueue(this);
                onActionQueueChanged?.Invoke(new ActionQueueEventArgs(ActionQueueEventType.ActionAdded, action));
                return true;
            }

            return false;
        }

        // 消耗队首行动
        public IAction ConsumeActionWithoutExecution()
        {
            if (actionQueue.Count > 0)
            {
                var action = actionQueue[0];
                actionQueue.RemoveAt(0); // 移除队首
                return action;
            }

            return null;
        }

        // 新增：消耗指定的行动
        public bool ConsumeSpecificAction(IAction action)
        {
            if (action != null && actionQueue.Contains(action))
            {
                actionQueue.Remove(action);
                return true;
            }

            return false;
        }

        // 查看队首行动
        public IAction PeekAction()
        {
            return actionQueue.FirstOrDefault();
        }

        public int GetActionCount()
        {
            return actionQueue.Count;
        }

        public List<IAction> GetAllActions()
        {
            return new List<IAction>(actionQueue);
        }

        public void Clear()
        {
            actionQueue.Clear();
            onActionQueueChanged?.Invoke(new ActionQueueEventArgs(ActionQueueEventType.QueueCleared, null));
        }

        public void SetMaxQueueSize(int size)
        {
            maxQueueSize = Mathf.Max(1, size);
        }

        public int GetMaxQueueSize()
        {
            return maxQueueSize;
        }

        public void TriggerBeforeActionExecutionEvent(IAction action)
        {
            onActionQueueChanged?.Invoke(new ActionQueueEventArgs(ActionQueueEventType.BeforeActionExecution, action));
        }

        public void TriggerActionConsumedEvent(IAction action)
        {
            onActionQueueChanged?.Invoke(new ActionQueueEventArgs(ActionQueueEventType.ActionConsumed, action));
        }

        // 获取指定行动的前驱行动（队列中位于其前面的行动）
        public IAction GetPredecessorAction(IAction action)
        {
            if (action == null || actionQueue.Count <= 1)
                return null;

            var index = actionQueue.IndexOf(action);
            if (index <= 0) // 如果是第一个行动或未找到，则没有前驱
                return null;

            return actionQueue[index - 1];
        }

        // 获取指定行动的后继行动（队列中位于其后面的行动）
        public IAction GetSuccessorAction(IAction action)
        {
            if (action == null || actionQueue.Count <= 1)
                return null;

            var index = actionQueue.IndexOf(action);
            if (index < 0 || index >= actionQueue.Count - 1) // 如果是最后一个行动或未找到，则没有后继
                return null;

            return actionQueue[index + 1];
        }

        // 获取指定行动在队列中的索引位置
        public int GetActionIndex(IAction action)
        {
            if (action == null)
                return -1;

            return actionQueue.IndexOf(action);
        }

        // 检查指定行动是否有前驱行动
        public bool HasPredecessor(IAction action)
        {
            return GetPredecessorAction(action) != null;
        }

        // 检查指定行动是否有后继行动
        public bool HasSuccessor(IAction action)
        {
            return GetSuccessorAction(action) != null;
        }
    }

    public enum ActionQueueEventType
    {
        ActionAdded,
        ActionConsumed,
        QueueCleared,
        BeforeActionExecution
    }

    public class ActionQueueEventArgs : EventArgs
    {
        public ActionQueueEventArgs(ActionQueueEventType eventType, IAction action)
        {
            EventType = eventType;
            Action = action;
        }

        public ActionQueueEventType EventType { get; private set; }
        public IAction Action { get; private set; }
    }
}