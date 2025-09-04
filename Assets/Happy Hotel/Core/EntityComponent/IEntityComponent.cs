namespace HappyHotel.Core.EntityComponent
{
    // 所有非Mono组件必须实现的基础接口
    public interface IEntityComponent
    {
        // 组件是否启用
        bool IsEnabled { get; set; }

        // 组件被添加到宿主时调用
        void OnAttach(EntityComponentContainer host);

        // 组件从宿主移除时调用
        void OnDetach();

        // 获取宿主对象
        EntityComponentContainer GetHost();
    }
}