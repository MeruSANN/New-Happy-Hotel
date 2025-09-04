using System;

namespace HappyHotel.Core.EntityComponent
{
    // EntityComponent初始化器接口
    public interface IEntityComponentInitializer
    {
        Type TargetContainerType { get; }
        void InitializeComponents(EntityComponentContainer container);
    }
}