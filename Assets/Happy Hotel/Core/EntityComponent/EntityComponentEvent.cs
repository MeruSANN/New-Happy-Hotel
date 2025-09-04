using System;

namespace HappyHotel.Core.EntityComponent
{
    // 组件事件类，用于组件间通信
    public class EntityComponentEvent
    {
        // 创建组件事件
        public EntityComponentEvent(string eventName, object sender, object data = null)
        {
            EventName = eventName ?? throw new ArgumentNullException(nameof(eventName));
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
            Data = data;
            IsCancelled = false;
            CancelReason = "";
        }

        // 事件名称
        public string EventName { get; }

        // 事件数据
        public object Data { get; }

        // 事件发送者（可以是IEntityComponent或EntityComponentContainer）
        public object Sender { get; }

        // 事件是否被取消
        public bool IsCancelled { get; private set; }

        // 取消原因
        public string CancelReason { get; private set; }

        // 取消事件
        public void Cancel(string reason = "")
        {
            IsCancelled = true;
            CancelReason = reason;
        }

        // 便利方法：获取发送者作为IEntityComponent
        public IEntityComponent GetSenderAsComponent()
        {
            return Sender as IEntityComponent;
        }

        // 便利方法：获取发送者作为EntityComponentContainer
        public EntityComponentContainer GetSenderAsContainer()
        {
            return Sender as EntityComponentContainer;
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
    public interface IEventListener : IEntityComponent
    {
        // 处理接收到的事件
        void OnEvent(EntityComponentEvent evt);
    }
}