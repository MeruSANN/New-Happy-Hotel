using HappyHotel.Equipment.Templates;
using UnityEngine;

namespace HappyHotel.Shop
{
	// 护甲获得增益护符+ 商店物品
	public class ArmorGainBonusCharmPlusShopItem : EquipmentShopItemBase
	{
		private int bonusStacks;

		protected override void OnTemplateSet()
		{
			base.OnTemplateSet();
			if (template is ArmorGainBonusCharmTemplate t)
			{
				bonusStacks = Mathf.Max(1, t.bonusStacks);
				itemName = t.itemName;
				description = t.description;
				itemIcon = t.icon;
				rarity = t.rarity;
			}
		}

		protected override string FormatDescriptionInternal(string formattedDescription)
		{
			return formattedDescription.Replace("{stacks}", bonusStacks.ToString());
		}
	}
}