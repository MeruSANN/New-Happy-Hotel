using System.Reflection;
using HappyHotel.Core.Registry;
using HappyHotel.Equipment.Templates;
using HappyHotel.Shop.Settings;
using UnityEngine;

namespace HappyHotel.Shop.Factory
{
    // 商店道具工厂基类，提供自动TypeId设置功能
    public abstract class ShopItemFactoryBase<TShopItem> : IShopItemFactory
        where TShopItem : ShopItemBase
    {
        public ShopItemBase Create(ItemTemplate template, IShopItemSetting setting = null)
        {
            var shopItemObject = new GameObject(GetShopItemName());

            var shopItem = shopItemObject.AddComponent<TShopItem>();

            // 自动设置TypeId
            AutoSetTypeId(shopItem);

            if (template) shopItem.SetTemplate(template);

            setting?.ConfigureShopItem(shopItem);

            shopItem.GeneratePrice();

            return shopItem;
        }

        private void AutoSetTypeId(ShopItemBase shopItem)
        {
            var attr = GetType().GetCustomAttribute<ShopItemRegistrationAttribute>();
            if (attr != null)
            {
                var typeId = TypeId.Create<ShopItemTypeId>(attr.TypeId);
                ((ITypeIdSettable<ShopItemTypeId>)shopItem).SetTypeId(typeId);
            }
        }

        protected virtual string GetShopItemName()
        {
            return typeof(TShopItem).Name;
        }
    }
}