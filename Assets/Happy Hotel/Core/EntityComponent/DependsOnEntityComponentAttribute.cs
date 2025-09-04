using System;

namespace HappyHotel.Core.EntityComponent
{
    // 组件依赖注解，用于声明组件的依赖关系
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class DependsOnEntityComponentAttribute : Attribute
    {
        // 创建组件依赖注解
        public DependsOnEntityComponentAttribute(Type requiredType, bool autoAdd = true)
        {
            RequiredType = requiredType;
            AutoAdd = autoAdd;
        }

        // 依赖的组件类型
        public Type RequiredType { get; }

        // 是否在添加组件时自动添加依赖
        public bool AutoAdd { get; }
    }
}