using System;
using System.Collections.Generic;
using System.Linq;
using HappyHotel.Core.Registry;
using HappyHotel.Equipment.Templates;
using HappyHotel.Shop.Factory;
using HappyHotel.Shop.Settings;

namespace HappyHotel.Shop
{
    public class ShopItemRegistry : RegistryBase<ShopItemBase, ShopItemTypeId, IShopItemFactory, ItemTemplate,
        IShopItemSetting>
    {
        private readonly Dictionary<ShopItemTypeId, ShopItemDescriptor> descriptors = new();

        protected override void OnRegister(RegistrationAttribute attr)
        {
            var type = GetType(attr.TypeId);
            descriptors[type] = new ShopItemDescriptor(type, attr.TemplatePath);
        }

        protected override Type GetRegistrationAttributeType()
        {
            return typeof(ShopItemRegistrationAttribute);
        }

        public List<ShopItemDescriptor> GetAllDescriptors()
        {
            return descriptors.Values.ToList();
        }

        public ShopItemDescriptor GetDescriptor(ShopItemTypeId id)
        {
            return descriptors[id];
        }

        #region Singleton

        private static ShopItemRegistry instance;

        public static ShopItemRegistry Instance
        {
            get
            {
                if (instance == null) instance = new ShopItemRegistry();
                return instance;
            }
        }

        #endregion
    }
}