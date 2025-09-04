using HappyHotel.Equipment.Templates;
using UnityEngine;

namespace HappyHotel.Shop
{
	// 购买后获得战之护符（攻击+护甲）的商店道具
    public class WarCharmShopItem : EquipmentShopItemBase
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


