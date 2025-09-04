using HappyHotel.Core.ValueProcessing;
using HappyHotel.Equipment.Templates;
using UnityEngine;

namespace HappyHotel.Shop
{
    // 购买后获得铁剑武器的商店道具
    public class IronSwordShopItem : EquipmentShopItemBase
    {
        // 武器伤害值
        [SerializeField] private EquipmentValue weaponDamageValue = new("武器伤害");

        public IronSwordShopItem()
        {
            weaponDamageValue.Initialize(this);
        }

        public int WeaponDamage => weaponDamageValue;

        protected override void OnTemplateSet()
        {
            base.OnTemplateSet();
            // 从武器商店道具模板中读取伤害值
            if (template is WeaponTemplate weaponTemplate) weaponDamageValue.SetBaseValue(weaponTemplate.weaponDamage);
        }

        public override bool CanAddToInventory()
        {
            // 检查基础条件
            if (!base.CanAddToInventory()) return false;
            // 这里我们允许拥有多个铁剑
            return true;
        }

        // 设置武器伤害值
        public void SetWeaponDamage(int damage)
        {
            weaponDamageValue.SetBaseValue(Mathf.Max(0, damage));
        }

        protected override string FormatDescriptionInternal(string formattedDescription)
        {
            return formattedDescription
                .Replace("{damage}", WeaponDamage.ToString());
        }
    }
}