using System;
using HappyHotel.Core.Registry;

namespace HappyHotel.Enemy
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class EnemyRegistrationAttribute : RegistrationAttribute
    {
        public EnemyRegistrationAttribute(string typeId, string templatePath = null) : base(typeId, templatePath)
        {
        }
    }
}