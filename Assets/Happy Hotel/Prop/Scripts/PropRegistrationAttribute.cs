using System;
using HappyHotel.Core.Registry;

namespace HappyHotel.Prop
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PropRegistrationAttribute : RegistrationAttribute
    {
        public PropRegistrationAttribute(string typeId, string templatePath) : base(typeId, templatePath)
        {
        }
    }
}