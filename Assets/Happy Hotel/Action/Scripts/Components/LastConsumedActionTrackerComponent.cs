using System;
using HappyHotel.Core.BehaviorComponent;
using UnityEngine;

namespace HappyHotel.Action.Components
{
    // 跟踪前一个消耗的行动的组件，用于检测前一个行动的类型
    [DependsOnComponent(typeof(ActionQueueComponent))]
    public class LastConsumedActionTrackerComponent : BehaviorComponentBase, IEventListener
    {
        private ActionQueueComponent monitoredActionQueue;
        public IAction LastConsumedAction { get; private set; }

        public override void OnAttach(BehaviorComponentContainer host)
        {
            base.OnAttach(host);

            // 尝试获取同容器中的ActionQueueComponent
            monitoredActionQueue = host.GetBehaviorComponent<ActionQueueComponent>();
            if (monitoredActionQueue != null) StartMonitoring();
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

        public override void OnDetach()
        {
            // 解除事件监听
            StopMonitoring();
            base.OnDetach();
        }

        // 前一个行动改变事件
        public event Action<IAction> onLastConsumedActionChanged;

        // 检查前一个消耗的行动是否为指定类型
        public bool IsLastConsumedActionOfType<T>() where T : class, IAction
        {
            return LastConsumedAction is T;
        }

        // 检查前一个消耗的行动是否为指定类型ID
        public bool IsLastConsumedActionOfTypeId(ActionTypeId typeId)
        {
            if (LastConsumedAction == null) return false;

            if (LastConsumedAction is ActionBase actionBase) return actionBase.TypeId.Equals(typeId);

            return false;
        }

        // 开始监听指定的ActionQueueComponent
        private void StartMonitoring()
        {
            if (monitoredActionQueue != null)
            {
                monitoredActionQueue.onActionQueueChanged += OnActionQueueChanged;
                Debug.Log("LastConsumedActionTrackerComponent: 开始监听ActionQueue");
            }
        }

        // 停止监听
        private void StopMonitoring()
        {
            if (monitoredActionQueue != null)
            {
                monitoredActionQueue.onActionQueueChanged -= OnActionQueueChanged;
                monitoredActionQueue = null;
                Debug.Log("LastConsumedActionTrackerComponent: 停止监听ActionQueue");
            }
        }

        // 处理ActionQueue变化事件
        private void OnActionQueueChanged(ActionQueueEventArgs args)
        {
            if (args.EventType == ActionQueueEventType.ActionConsumed)
            {
                LastConsumedAction = args.Action;
                Debug.Log($"记录前一个消耗的行动: {args.Action?.GetType().Name}");

                // 触发前一个行动改变事件
                onLastConsumedActionChanged?.Invoke(LastConsumedAction);
            }
        }
    }
}