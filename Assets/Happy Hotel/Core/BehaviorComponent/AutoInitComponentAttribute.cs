using System;

namespace HappyHotel.Core.BehaviorComponent
{
    // 用于标记BehaviorComponentContainer需要自动初始化的BehaviorComponent
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class AutoInitComponentAttribute : Attribute
    {
        // 标记需要自动初始化的BehaviorComponent
        public AutoInitComponentAttribute(Type componentType, params object[] initParams)
        {
            if (!typeof(IBehaviorComponent).IsAssignableFrom(componentType))
                throw new ArgumentException($"类型 {componentType.Name} 必须实现 IBehaviorComponent 接口");

            ComponentType = componentType;
            InitParams = initParams;
        }

        public Type ComponentType { get; private set; }
        public object[] InitParams { get; private set; }
    }
}