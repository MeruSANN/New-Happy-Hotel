using System;

namespace HappyHotel.Core.EntityComponent
{
    // 用于标记EntityComponentContainer需要自动初始化的EntityComponent
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class AutoInitEntityComponentAttribute : Attribute
    {
        // 标记需要自动初始化的EntityComponent
        public AutoInitEntityComponentAttribute(Type componentType, params object[] initParams)
        {
            if (!typeof(IEntityComponent).IsAssignableFrom(componentType))
                throw new ArgumentException($"类型 {componentType.Name} 必须实现 IEntityComponent 接口");

            ComponentType = componentType;
            InitParams = initParams;
        }

        public Type ComponentType { get; private set; }
        public object[] InitParams { get; private set; }
    }
}