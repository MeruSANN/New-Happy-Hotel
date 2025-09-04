using System;

namespace HappyHotel.Core.BehaviorComponent
{
    // 组件初始化器接口
    public interface IComponentInitializer
    {
        Type TargetContainerType { get; }
        void InitializeComponents(BehaviorComponentContainer container);
    }
}