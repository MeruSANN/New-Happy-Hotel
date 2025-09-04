using System;

namespace HappyHotel.Core.BehaviorComponent
{
    // 组件初始化器注册属性
    [AttributeUsage(AttributeTargets.Class)]
    public class ComponentInitializerAttribute : Attribute
    {
        public ComponentInitializerAttribute(Type targetContainerType, params Type[] componentTypes)
        {
            TargetContainerType = targetContainerType;
            ComponentTypes = componentTypes;
        }

        public Type TargetContainerType { get; }
        public Type[] ComponentTypes { get; }
    }
}