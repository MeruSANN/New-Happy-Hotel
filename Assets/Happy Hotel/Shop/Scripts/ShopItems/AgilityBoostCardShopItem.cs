using HappyHotel.Card.Templates;
using UnityEngine;

namespace HappyHotel.Shop
{
	// 购买后获得提升敏捷上限卡牌的商店道具
	public class AgilityBoostCardShopItem : CardShopItemBase
	{
		private int agilityIncrease;

		protected override void OnTemplateSet()
		{
			base.OnTemplateSet();
			if (template is AgilityBoostCardTemplate boostTemplate)
			{
				agilityIncrease = Mathf.Max(0, boostTemplate.agilityIncrease);
			}
		}

		protected override string FormatDescriptionInternal(string formattedDescription)
		{
			return formattedDescription.Replace("{agility}", agilityIncrease.ToString());
		}
	}
}



