using System;
using UnityEngine;

namespace HappyHotel.Core.Singleton
{
    // 用于声明单例类的依赖关系
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class SingletonInitializationDependencyAttribute : Attribute
    {
        public SingletonInitializationDependencyAttribute(Type dependencyType, bool isRequired = true)
        {
            if (!typeof(MonoBehaviour).IsAssignableFrom(dependencyType))
                throw new ArgumentException($"依赖类型 {dependencyType.Name} 必须继承自MonoBehaviour");

            DependencyType = dependencyType;
            IsRequired = isRequired;
        }

        public Type DependencyType { get; }
        public bool IsRequired { get; }
    }
}