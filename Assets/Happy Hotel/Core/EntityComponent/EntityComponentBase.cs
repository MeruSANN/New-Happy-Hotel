namespace HappyHotel.Core.EntityComponent
{
    // 非Mono组件的抽象基类，提供基本实现
    public abstract class EntityComponentBase : IEntityComponent
    {
        // 宿主对象引用
        protected EntityComponentContainer host;

        // 组件是否启用
        public bool IsEnabled { get; set; } = true;

        // 当组件被添加到宿主时调用
        public virtual void OnAttach(EntityComponentContainer host)
        {
            this.host = host;
        }

        // 当组件从宿主移除时调用
        public virtual void OnDetach()
        {
            host = null;
        }

        // 获取宿主对象
        public EntityComponentContainer GetHost()
        {
            return host;
        }

        // 生命周期事件

        #region Lifecycle Events

        // 初始化时调用（在构造和OnAttach后自动调用）
        public virtual void OnInitialize()
        {
        }

        // 更新时调用（需要宿主手动调用UpdateComponents）
        public virtual void OnUpdate()
        {
        }

        // 销毁时调用（在Dispose时自动调用）
        public virtual void OnDestroy()
        {
        }

        #endregion
    }
}