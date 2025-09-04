using HappyHotel.Equipment.Templates;

namespace HappyHotel.Shop
{
    // 触发增幅护符+ 商店物品
    public class PropBoostCharmPlusShopItem : EquipmentShopItemBase
    {
        private int bonus;

        public int Bonus => bonus;

        protected override void OnTemplateSet()
        {
            base.OnTemplateSet();
            if (template is PropBoostCharmTemplate t) bonus = t.buffBonus;
        }

        protected override string FormatDescriptionInternal(string formattedDescription)
        {
            return formattedDescription.Replace("{bonus}", bonus.ToString());
        }
    }
}