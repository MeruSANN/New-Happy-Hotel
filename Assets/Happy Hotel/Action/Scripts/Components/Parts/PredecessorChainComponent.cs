using System.Collections.Generic;
using HappyHotel.Core.EntityComponent;

namespace HappyHotel.Action.Components.Parts
{
    // 前驱链式执行组件，自动注册到前驱行动的onBuildChain事件
    public class PredecessorChainComponent : EntityComponentBase, IEventListener
    {
        // 这个组件需要知道它所属的行动 (parentAction)
        private ActionBase parentAction;

        // 当前注册的前驱行动
        private ActionBase registeredPredecessor;

        public override void OnAttach(EntityComponentContainer host)
        {
            base.OnAttach(host);

            parentAction = host as ActionBase;
        }

        // 实现IEventListener接口，处理事件
        public void OnEvent(EntityComponentEvent evt)
        {
            if (evt.EventName == "SetActionQueue")
                // 当行动被设置到队列时，尝试注册到前驱行动
                TryRegisterToPredecessor();
        }

        // 尝试注册到前驱行动
        private void TryRegisterToPredecessor()
        {
            if (parentAction == null) return;

            var actionQueue = parentAction.GetActionQueue();
            if (actionQueue == null) return;

            // 获取前驱行动
            var predecessor = actionQueue.GetPredecessorAction(parentAction);
            if (predecessor is ActionBase predecessorAction)
            {
                // 如果前驱行动发生变化，先取消之前的注册
                if (registeredPredecessor != null && registeredPredecessor != predecessorAction)
                    UnregisterFromPredecessor();

                // 注册到新的前驱行动
                if (registeredPredecessor != predecessorAction) RegisterToPredecessor(predecessorAction);
            }
            else
            {
                // 如果没有前驱行动，取消之前的注册
                UnregisterFromPredecessor();
            }
        }

        // 注册到前驱行动
        private void RegisterToPredecessor(ActionBase predecessorAction)
        {
            if (predecessorAction == null || parentAction == null) return;

            registeredPredecessor = predecessorAction;
            predecessorAction.onBuildChain += AppendToActionChain;
        }

        // 从前驱行动取消注册
        private void UnregisterFromPredecessor()
        {
            if (registeredPredecessor != null && parentAction != null)
            {
                registeredPredecessor.onBuildChain -= AppendToActionChain;
                registeredPredecessor = null;
            }
        }

        // 当前驱行动构建链时，将自己也加入链中
        private void AppendToActionChain(Queue<IAction> chain)
        {
            if (parentAction != null) parentAction.BuildActionChain(chain);
        }

        // 当这个组件被销毁时，需要取消订阅以防内存泄漏
        public override void OnDestroy()
        {
            base.OnDestroy();
            UnregisterFromPredecessor();
        }
    }
}