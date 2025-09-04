using HappyHotel.Action.Components;

namespace HappyHotel.Action
{
    // 定义Action接口，替代原有的枚举类型
    public interface IAction
    {
        // 执行行动
        void Execute();

        // 获取所属的ActionQueue
        ActionQueueComponent GetActionQueue();

        // 设置所属的ActionQueue
        void SetActionQueue(ActionQueueComponent queue);
    }
}