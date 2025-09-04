using HappyHotel.Core.ValueProcessing;
using HappyHotel.Equipment.Templates;
using UnityEngine;

namespace HappyHotel.Shop
{
    // 铁剑+ 商店物品
    public class IronSwordPlusShopItem : EquipmentShopItemBase
    {
        [SerializeField] private EquipmentValue weaponDamageValue = new("武器伤害");

        public IronSwordPlusShopItem()
        {
            weaponDamageValue.Initialize(this);
        }

        public int WeaponDamage => weaponDamageValue;

        protected override void OnTemplateSet()
        {
            base.OnTemplateSet();
            if (template is WeaponTemplate weaponTemplate) weaponDamageValue.SetBaseValue(weaponTemplate.weaponDamage);
        }

        public override bool CanAddToInventory()
        {
            if (!base.CanAddToInventory()) return false;
            return true;
        }

        public void SetWeaponDamage(int damage)
        {
            weaponDamageValue.SetBaseValue(Mathf.Max(0, damage));
        }

        protected override string FormatDescriptionInternal(string formattedDescription)
        {
            return formattedDescription.Replace("{damage}", WeaponDamage.ToString());
        }
    }
}