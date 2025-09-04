using System.Collections.Generic;
using HappyHotel.Core.BehaviorComponent;
using UnityEngine;

namespace HappyHotel.Action.Components
{
    // 行动循环组件，当ActionQueue被消耗时自动添加预设的行动
    [DependsOnComponent(typeof(ActionQueueComponent))]
    public class ActionCycleComponent : BehaviorComponentBase
    {
        // 预设的行动列表
        [SerializeField] private List<IAction> actionCycle = new();

        // ActionQueue组件引用
        private ActionQueueComponent actionQueueComponent;

        // 当前循环索引
        private int currentIndex;

        // 是否启用自动循环
        [SerializeField] private bool enableAutoCycle = true;

        public override void OnAttach(BehaviorComponentContainer host)
        {
            base.OnAttach(host);

            // 获取ActionQueueComponent
            actionQueueComponent = host.GetBehaviorComponent<ActionQueueComponent>();

            if (actionQueueComponent != null)
                // 订阅ActionQueue事件
                actionQueueComponent.onActionQueueChanged += OnActionQueueChanged;
            else
                Debug.LogError("ActionCycleComponent: 未找到ActionQueueComponent!");
        }

        public override void OnDetach()
        {
            // 取消订阅事件
            if (actionQueueComponent != null) actionQueueComponent.onActionQueueChanged -= OnActionQueueChanged;

            base.OnDetach();
        }

        public override void OnStart()
        {
            base.OnStart();

            InitializeActionQueue();
        }

        // 处理ActionQueue变化事件
        private void OnActionQueueChanged(ActionQueueEventArgs args)
        {
            // 当行动被消耗且启用自动循环时，添加下一个行动
            if (args.EventType == ActionQueueEventType.ActionConsumed && enableAutoCycle) AddNextAction();
        }

        // 添加下一个行动到队列
        private void AddNextAction()
        {
            if (actionCycle.Count == 0)
            {
                Debug.LogWarning("ActionCycleComponent: 行动循环列表为空!");
                return;
            }

            // 获取当前索引的行动
            var nextAction = actionCycle[currentIndex];

            if (nextAction != null && actionQueueComponent != null)
            {
                // 添加行动到队列
                var success = actionQueueComponent.AddAction(nextAction);

                if (success)
                {
                    Debug.Log($"ActionCycleComponent: 添加行动 {nextAction.GetType().Name} (索引: {currentIndex})");

                    // 移动到下一个索引，循环到列表开头
                    currentIndex = (currentIndex + 1) % actionCycle.Count;
                }
                else
                {
                    Debug.LogWarning("ActionCycleComponent: 无法添加行动到队列，队列可能已满!");
                }
            }
        }

        // 设置行动循环列表
        public void SetActionCycle(List<IAction> actions)
        {
            actionCycle = new List<IAction>(actions);
            currentIndex = 0;
        }

        // 添加行动到循环列表
        public void AddActionToCycle(IAction action)
        {
            if (action != null) actionCycle.Add(action);
        }

        // 移除循环列表中的行动
        public void RemoveActionFromCycle(IAction action)
        {
            if (actionCycle.Contains(action))
            {
                var removedIndex = actionCycle.IndexOf(action);
                actionCycle.Remove(action);

                // 调整当前索引
                if (currentIndex > removedIndex)
                    currentIndex--;
                else if (currentIndex >= actionCycle.Count && actionCycle.Count > 0) currentIndex = 0;
            }
        }

        // 清空循环列表
        public void ClearActionCycle()
        {
            actionCycle.Clear();
            currentIndex = 0;
        }

        // 获取当前循环列表
        public List<IAction> GetActionCycle()
        {
            return new List<IAction>(actionCycle);
        }

        // 设置是否启用自动循环
        public void SetAutoCycleEnabled(bool enabled)
        {
            enableAutoCycle = enabled;
        }

        // 获取是否启用自动循环
        public bool IsAutoCycleEnabled()
        {
            return enableAutoCycle;
        }

        // 获取当前索引
        public int GetCurrentIndex()
        {
            return currentIndex;
        }

        // 手动触发添加下一个行动
        private void InitializeActionQueue()
        {
            for (var i = 0; i < actionCycle.Count; i++)
                AddNextAction();
        }

        // 重置循环索引到开头
        public void ResetCycleIndex()
        {
            currentIndex = 0;
        }
    }
}