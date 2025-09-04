using System;

namespace HappyHotel.Core.BehaviorComponent
{
    // 组件事件类，用于组件间通信
    public class BehaviorComponentEvent
    {
        // 创建组件事件
        public BehaviorComponentEvent(string eventName, object sender, object data = null)
        {
            EventName = eventName ?? throw new ArgumentNullException(nameof(eventName));
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
            Data = data;
        }

        // 事件名称
        public string EventName { get; }

        // 事件数据
        public object Data { get; }

        // 事件发送者（可以是IBehaviorComponent或BehaviorComponentContainer）
        public object Sender { get; }

        // 便利方法：获取发送者作为IBehaviorComponent
        public IBehaviorComponent GetSenderAsComponent()
        {
            return Sender as IBehaviorComponent;
        }

        // 便利方法：获取发送者作为BehaviorComponentContainer
        public BehaviorComponentContainer GetSenderAsContainer()
        {
            return Sender as BehaviorComponentContainer;
        }

        // 便利方法：检查发送者是否为指定类型
        public bool IsSenderOfType<T>()
        {
            return Sender is T;
        }

        // 便利方法：尝试获取发送者作为指定类型
        public T GetSenderAs<T>() where T : class
        {
            return Sender as T;
        }
    }

    // 事件监听接口
    public interface IEventListener : IBehaviorComponent
    {
        // 处理接收到的事件
        void OnEvent(BehaviorComponentEvent evt);
    }
}