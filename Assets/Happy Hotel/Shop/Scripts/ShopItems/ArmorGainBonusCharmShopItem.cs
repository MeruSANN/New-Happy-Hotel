using HappyHotel.Equipment.Templates;
using UnityEngine;

namespace HappyHotel.Shop
{
	// 购买后获得护甲获得增益护符的商店物品
    public class ArmorGainBonusCharmShopItem : EquipmentShopItemBase
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


