using HappyHotel.Core.Registry;
using HappyHotel.Equipment.Templates;
using HappyHotel.Shop.Factory;
using HappyHotel.Shop.Settings;
using UnityEngine;

namespace HappyHotel.Shop
{
    public class ShopItemResourceManager : ResourceManagerBase<ShopItemBase, ShopItemTypeId, IShopItemFactory,
        ItemTemplate, IShopItemSetting>
    {
        protected override void LoadTypeResources(ShopItemTypeId type)
        {
            var descriptor = (registry as ShopItemRegistry)!.GetDescriptor(type);

            var template = Resources.Load<ItemTemplate>(descriptor.TemplatePath);
            if (template != null)
                templateCache[descriptor.TypeId] = template;
            else
                Debug.LogWarning($"无法加载商店道具模板: {descriptor.TemplatePath}");
        }
    }
}