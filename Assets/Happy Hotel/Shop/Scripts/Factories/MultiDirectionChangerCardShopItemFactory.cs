using UnityEngine;

namespace HappyHotel.Shop.Factory
{
    [ShopItemRegistration(
        "MultiDirectionChangerCard",
        "Templates/Multi Direction Changer Card Template")]
    public class MultiDirectionChangerCardShopItemFactory : ShopItemFactoryBase<MultiDirectionChangerCardShopItem>
    {
    }
}