using System;
using HappyHotel.Core.Registry;

namespace HappyHotel.Character
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class CharacterRegistrationAttribute : RegistrationAttribute
    {
        public CharacterRegistrationAttribute(string typeId, string templatePath = null) : base(typeId, templatePath)
        {
        }
    }
}