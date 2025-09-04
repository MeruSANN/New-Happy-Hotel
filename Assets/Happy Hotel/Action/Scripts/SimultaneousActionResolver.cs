using System.Collections.Generic;
using System.Linq;
using HappyHotel.Action.Components;
using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.EntityComponent;
using IEventListener = HappyHotel.Core.EntityComponent.IEventListener;

namespace HappyHotel.Action
{
    // 同时行动结算器，处理多个对象同时执行行动的情况
    public class SimultaneousActionResolver
    {
        // 新方法，用于处理一对行动的解析
        public void ResolveSinglePair(IAction actionA, IAction actionB, BehaviorComponentContainer partyA,
            BehaviorComponentContainer partyB)
        {
            var consumedActionInfos = new List<(IAction action, ActionQueueComponent actionQueue)>();
            var componentsToExecute = new List<ComponentExecutionInfo>();

            // 处理行动A - 使用行动的真实所有者，而不是传入的party
            if (actionA != null)
            {
                var actionAQueue = actionA.GetActionQueue();
                if (actionAQueue != null)
                {
                    consumedActionInfos.Add((actionA, actionAQueue));
                    CollectEntityComponentsFromAction(actionA, actionAQueue, componentsToExecute);
                }
            }

            // 处理行动B - 使用行动的真实所有者，而不是传入的party
            if (actionB != null)
            {
                var actionBQueue = actionB.GetActionQueue();
                if (actionBQueue != null)
                {
                    consumedActionInfos.Add((actionB, actionBQueue));
                    CollectEntityComponentsFromAction(actionB, actionBQueue, componentsToExecute);
                }
            }

            if (consumedActionInfos.Count == 0) return;

            // 按正确顺序执行：发出执行前事件 → 执行行动本身 → 处理组件 → 发出执行后事件
            foreach (var (action, actionQueue) in consumedActionInfos)
            {
                // 1. 发出行动执行前事件
                actionQueue.TriggerBeforeActionExecutionEvent(action);

                // 2. 执行行动本身
                action.Execute();
            }

            // 3. 按组件类型优先级执行所有组件
            ExecuteComponentsByPriority(componentsToExecute);

            // 4. 发出所有行动的执行完成事件
            foreach (var (action, actionQueue) in consumedActionInfos) actionQueue.TriggerActionConsumedEvent(action);
        }

        // 从Action中收集所有EntityComponent
        private void CollectEntityComponentsFromAction(IAction action, ActionQueueComponent actionQueue,
            List<ComponentExecutionInfo> componentsToExecute)
        {
            if (action is not ActionBase actionBase) return;

            // 获取Action中所有的IEventListener组件
            var listeners = actionBase.GetEntityComponents<IEventListener>();

            foreach (var listener in listeners)
                if (listener.IsEnabled)
                {
                    var priority = PriorityEventExecutor.GetComponentPriority(listener.GetType());
                    componentsToExecute.Add(new ComponentExecutionInfo(listener, actionQueue, priority));
                }
        }

        // 按优先级执行所有组件
        private void ExecuteComponentsByPriority(List<ComponentExecutionInfo> componentsToExecute)
        {
            // 按优先级分组并排序（数值越小优先级越高）
            var priorityGroups = componentsToExecute
                .GroupBy(info => info.priority)
                .OrderBy(group => group.Key);

            // 为每个行动队列创建独立的事件对象，确保同一ActionQueue的组件共享事件状态
            var eventsByActionQueue = new Dictionary<ActionQueueComponent, EntityComponentEvent>();

            // 按优先级顺序执行
            foreach (var priorityGroup in priorityGroups)
            foreach (var componentInfo in priorityGroup)
            {
                // 获取或创建该ActionQueue对应的事件对象
                if (!eventsByActionQueue.TryGetValue(componentInfo.actionQueue, out var executeEvent))
                {
                    executeEvent = new EntityComponentEvent("Execute", componentInfo.component.GetHost(),
                        componentInfo.actionQueue);
                    eventsByActionQueue[componentInfo.actionQueue] = executeEvent;
                }

                // 检查事件是否已被取消（例如被晕眩）
                if (executeEvent.IsCancelled) continue;

                componentInfo.component.OnEvent(executeEvent);
            }
        }

        // 组件执行信息结构体
        private struct ComponentExecutionInfo
        {
            public readonly IEventListener component;
            public readonly ActionQueueComponent actionQueue;
            public readonly int priority;

            public ComponentExecutionInfo(IEventListener component, ActionQueueComponent actionQueue, int priority)
            {
                this.component = component;
                this.actionQueue = actionQueue;
                this.priority = priority;
            }
        }
    }
}