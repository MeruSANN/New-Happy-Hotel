namespace HappyHotel.Core.BehaviorComponent
{
    // 所有组件必须实现的基础接口
    public interface IBehaviorComponent
    {
        // 组件是否启用
        bool IsEnabled { get; set; }

        // 组件被添加到宿主时调用
        void OnAttach(BehaviorComponentContainer host);

        // 组件从宿主移除时调用
        void OnDetach();

        // 获取宿主对象
        BehaviorComponentContainer GetHost();
    }
}