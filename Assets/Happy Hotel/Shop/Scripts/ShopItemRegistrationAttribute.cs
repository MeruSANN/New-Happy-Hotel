using System;
using HappyHotel.Core.Registry;

namespace HappyHotel.Shop
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ShopItemRegistrationAttribute : RegistrationAttribute
    {
        public ShopItemRegistrationAttribute(string typeId, string templatePath = null) : base(typeId, templatePath)
        {
        }
    }
}