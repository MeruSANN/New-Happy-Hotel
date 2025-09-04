using System;

namespace HappyHotel.Core.EntityComponent
{
    // EntityComponent初始化器注册属性
    [AttributeUsage(AttributeTargets.Class)]
    public class EntityComponentInitializerAttribute : Attribute
    {
        public EntityComponentInitializerAttribute(Type targetContainerType, params Type[] componentTypes)
        {
            TargetContainerType = targetContainerType;
            ComponentTypes = componentTypes;
        }

        public Type TargetContainerType { get; }
        public Type[] ComponentTypes { get; }
    }
}