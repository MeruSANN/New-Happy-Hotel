using UnityEngine;

namespace HappyHotel.Shop.Factory
{
    [ShopItemRegistration(
        "Armor",
        "Templates/Armor Card Template")]
    public class ArmorCardShopItemFactory : ShopItemFactoryBase<ArmorCardShopItem>
    {
    }
}