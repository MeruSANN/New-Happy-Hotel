using HappyHotel.Equipment.Templates;
using UnityEngine;

namespace HappyHotel.Shop
{
	// 购买后获得提升当关费用上限饰品的商店道具
    public class CostCapCharmShopItem : EquipmentShopItemBase
    {
        private int maxCostBonus;

        protected override void OnTemplateSet()
        {
            base.OnTemplateSet();
            if (template is CostCapCharmTemplate t)
            {
                maxCostBonus = Mathf.Max(0, t.maxCostBonus);
                itemName = t.itemName;
                description = t.description;
                itemIcon = t.icon;
                rarity = t.rarity;
            }
        }

        protected override string FormatDescriptionInternal(string formattedDescription)
        {
            return formattedDescription.Replace("{maxCostBonus}", maxCostBonus.ToString());
        }
    }
}


