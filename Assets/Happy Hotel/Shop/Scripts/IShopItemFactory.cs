using HappyHotel.Core.Registry;
using HappyHotel.Equipment.Templates;
using HappyHotel.Shop.Settings;

namespace HappyHotel.Shop.Factory
{
    public interface IShopItemFactory : IFactory<ShopItemBase, ItemTemplate, IShopItemSetting>
    {
    }
}