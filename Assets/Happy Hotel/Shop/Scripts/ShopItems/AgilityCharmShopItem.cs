using HappyHotel.Equipment.Templates;
using UnityEngine;

namespace HappyHotel.Shop
{
	// 购买后获得敏捷护符装备的商店道具
	public class AgilityCharmShopItem : EquipmentShopItemBase
	{
		private int agilityIncrease;

		protected override void OnTemplateSet()
		{
			base.OnTemplateSet();
			if (template is AgilityCharmTemplate charmTemplate)
			{
				agilityIncrease = Mathf.Max(0, charmTemplate.agilityIncrease);
			}
		}

		protected override string FormatDescriptionInternal(string formattedDescription)
		{
			return formattedDescription.Replace("{agility}", agilityIncrease.ToString());
		}
	}
}



