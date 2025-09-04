using System;
using HappyHotel.Core.Registry;

namespace HappyHotel.Device
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DeviceRegistrationAttribute : RegistrationAttribute
    {
        public DeviceRegistrationAttribute(string typeId, string templatePath) : base(typeId, templatePath)
        {
        }
    }
}