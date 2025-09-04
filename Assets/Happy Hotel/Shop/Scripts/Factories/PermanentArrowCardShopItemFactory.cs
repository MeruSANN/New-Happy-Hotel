using UnityEngine;

namespace HappyHotel.Shop.Factory
{
    [ShopItemRegistration(
        "PermanentArrow",
        "Templates/Permanent Arrow Card Template")]
    public class PermanentArrowCardShopItemFactory : ShopItemFactoryBase<PermanentArrowCardShopItem>
    {
    }
}