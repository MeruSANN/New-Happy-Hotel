using HappyHotel.Equipment.Templates;
using UnityEngine;

namespace HappyHotel.Shop
{
	// 战之护符+ 商店物品
	public class WarCharmPlusShopItem : EquipmentShopItemBase
	{
		private int weaponDamage;
		private int armorAmount;

		protected override void OnTemplateSet()
		{
			base.OnTemplateSet();
			if (template is WarCharmTemplate t)
			{
				weaponDamage = Mathf.Max(0, t.weaponDamage);
				armorAmount = Mathf.Max(0, t.armorAmount);
				itemName = t.itemName;
				description = t.description;
				itemIcon = t.icon;
				rarity = t.rarity;
			}
		}

		protected override string FormatDescriptionInternal(string formattedDescription)
		{
			return formattedDescription
				.Replace("{damage}", weaponDamage.ToString())
				.Replace("{armor}", armorAmount.ToString());
		}
	}
}