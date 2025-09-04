using System.Collections.Generic;
using HappyHotel.Action.Components;
using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.Singleton;
using HappyHotel.GameManager;
using UnityEngine;

namespace HappyHotel.Action
{
    [ManagedSingleton]
    public class ActionChainResolver : SingletonBase<ActionChainResolver>
    {
        // 静态锁，防止对同一次碰撞的重复处理
        private static readonly HashSet<string> processingCollisions = new();

        private SimultaneousActionResolver simultaneousResolver;

        protected override void OnSingletonAwake()
        {
            simultaneousResolver = new SimultaneousActionResolver();

            // 订阅时钟系统的行动链信号
            if (ClockSystem.Instance != null) ClockSystem.Instance.onActionChainTick.AddListener(OnActionChainTick);
        }

        public string Request(BehaviorComponentContainer partyA, BehaviorComponentContainer partyB)
        {
            // 生成顺序无关的唯一碰撞ID
            var id1 = partyA.GetInstanceID();
            var id2 = partyB.GetInstanceID();
            var collisionId = id1 < id2 ? $"{id1}-{id2}" : $"{id2}-{id1}";

            // 检查锁
            if (processingCollisions.Contains(collisionId)) return null; // 正在处理中，直接返回

            // 加锁
            processingCollisions.Add(collisionId);

            // 开始解析
            ResolveActionChains(partyA, partyB);

            return collisionId;
        }

        private void ResolveActionChains(BehaviorComponentContainer partyA, BehaviorComponentContainer partyB)
        {
            var chainA = new Queue<IAction>();
            var chainB = new Queue<IAction>();

            var queueCompA = partyA.GetBehaviorComponent<ActionQueueComponent>();
            var queueCompB = partyB.GetBehaviorComponent<ActionQueueComponent>();
            var consumerCompA = partyA.GetBehaviorComponent<ActionConsumerComponent>();
            var consumerCompB = partyB.GetBehaviorComponent<ActionConsumerComponent>();

            // 正确逻辑：消费者消费对方的行动队列，但由消费者自己执行
            // A作为消费者，消费B的行动队列，但由A执行
            if (consumerCompA != null && queueCompB != null && queueCompB.GetActionCount() > 0)
            {
                var startActionB = queueCompB.PeekAction();
                if (startActionB is ActionBase startActionBaseB)
                    startActionBaseB.BuildActionChain(chainA); // A消费B的行动，放入chainA由A执行
            }

            // B作为消费者，消费A的行动队列，但由B执行
            if (consumerCompB != null && queueCompA != null && queueCompA.GetActionCount() > 0)
            {
                var startActionA = queueCompA.PeekAction();
                if (startActionA is ActionBase startActionBaseA)
                    startActionBaseA.BuildActionChain(chainB); // B消费A的行动，放入chainB由B执行
            }

            // 提交给时钟系统执行
            if (ClockSystem.Instance != null)
                ClockSystem.Instance.SubmitActionChainExecution(chainA, chainB, partyA, partyB);
            else
                // 如果时钟系统不可用，使用原来的执行方式
                ExecutionLoop(chainA, chainB, partyA, partyB);
        }

        private void ExecutionLoop(Queue<IAction> chainA, Queue<IAction> chainB, BehaviorComponentContainer partyA,
            BehaviorComponentContainer partyB)
        {
            while (chainA.Count > 0 || chainB.Count > 0)
            {
                var actionA = chainA.Count > 0 ? chainA.Dequeue() : null;
                var actionB = chainB.Count > 0 ? chainB.Dequeue() : null;
                actionA?.GetActionQueue()?.ConsumeSpecificAction(actionA);
                actionB?.GetActionQueue()?.ConsumeSpecificAction(actionB);

                simultaneousResolver.ResolveSinglePair(actionA, actionB, partyA, partyB);
            }

            Debug.Log("Action chains resolved.");
        }

        public void ReleaseCollisionLock(string collisionId)
        {
            if (collisionId != null && processingCollisions.Contains(collisionId))
                processingCollisions.Remove(collisionId);
        }

        // 执行具体的行动对
        public void ExecuteActions(IAction actionA, IAction actionB,
            BehaviorComponentContainer partyA, BehaviorComponentContainer partyB)
        {
            // 消费行动
            actionA?.GetActionQueue()?.ConsumeSpecificAction(actionA);
            actionB?.GetActionQueue()?.ConsumeSpecificAction(actionB);

            // 使用simultaneousResolver解析行动对
            simultaneousResolver.ResolveSinglePair(actionA, actionB, partyA, partyB);
        }

        // 响应时钟系统的行动链信号
        private void OnActionChainTick()
        {
            // 执行下一组行动
            if (ClockSystem.Instance != null) ClockSystem.Instance.ExecuteNextActions();
        }

        // 用于测试清理
        public static void ClearCollisionLocks()
        {
            processingCollisions.Clear();
        }
    }
}