using System;

namespace HappyHotel.Core.Registry
{
    // 所有注册特性的抽象基类
    [AttributeUsage(AttributeTargets.Class)]
    public abstract class RegistrationAttribute : Attribute
    {
        protected RegistrationAttribute(string typeId, string templatePath = null)
        {
            TypeId = typeId;
            TemplatePath = templatePath;
        }

        public new string TypeId { get; }
        public string TemplatePath { get; }
    }
}