using UnityEngine;

namespace HappyHotel.Shop.Factory
{
    [ShopItemRegistration(
        "DirectionChangerCard",
        "Templates/Direction Changer Card Template")]
    public class DirectionChangerCardShopItemFactory : ShopItemFactoryBase<DirectionChangerCardShopItem>
    {
    }
}