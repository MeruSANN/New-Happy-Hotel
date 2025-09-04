using UnityEngine;

namespace HappyHotel.Shop.Factory
{
	[ShopItemRegistration(
		"DirectionPack",
		"Templates/Direction Pack Card Template")]
	public class DirectionPackCardShopItemFactory : ShopItemFactoryBase<DirectionPackCardShopItem>
	{
	}
}

