using System;
using HappyHotel.Core.Registry;

namespace HappyHotel.Card
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CardRegistrationAttribute : RegistrationAttribute
    {
        public CardRegistrationAttribute(string typeId, string templatePath) : base(typeId, templatePath)
        {
        }
    }
}