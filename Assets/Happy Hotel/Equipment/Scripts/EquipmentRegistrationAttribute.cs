using System;
using HappyHotel.Core.Registry;

namespace HappyHotel.Equipment
{
    // 武器注册特性
    // 用于标记 IWeaponFactory 实现类，并提供 TypeId 和可选的 TemplatePath
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class EquipmentRegistrationAttribute : RegistrationAttribute
    {
        public EquipmentRegistrationAttribute(string typeId, string templatePath = null)
            : base(typeId, templatePath)
        {
        }
    }
}