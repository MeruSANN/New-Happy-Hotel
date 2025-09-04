using HappyHotel.Core.ValueProcessing;
using HappyHotel.Equipment.Templates;
using UnityEngine;

namespace HappyHotel.Shop
{
    // 轻甲+ 商店物品
    public class LightArmorPlusShopItem : EquipmentShopItemBase
    {
        [SerializeField] private EquipmentValue armorAmount = new("护甲值");

        public LightArmorPlusShopItem()
        {
            armorAmount.Initialize(this);
        }

        public int ArmorAmount => armorAmount;

        public void SetArmorAmount(int newArmorAmount)
        {
            armorAmount.SetBaseValue(Mathf.Max(0, newArmorAmount));
        }

        protected override void OnTemplateSet()
        {
            base.OnTemplateSet();
            if (template is ArmorTemplate armorTemplate) armorAmount.SetBaseValue(armorTemplate.armorAmount);
        }

        public override bool CanAddToInventory()
        {
            if (!base.CanAddToInventory()) return false;
            return true;
        }

        protected override string FormatDescriptionInternal(string formattedDescription)
        {
            return formattedDescription.Replace("{armor}", armorAmount.ToString());
        }
    }
}