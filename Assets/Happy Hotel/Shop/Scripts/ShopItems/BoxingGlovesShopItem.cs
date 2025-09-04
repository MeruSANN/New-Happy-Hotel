using HappyHotel.Core.ValueProcessing;
using HappyHotel.Equipment.Templates;
using UnityEngine;
using UnityEngine.Serialization;

namespace HappyHotel.Shop
{
    // 购买后获得格斗拳套装备的商店道具
    public class BoxingGlovesShopItem : EquipmentShopItemBase
    {
        // 第一次攻击伤害值
        private EquipmentValue attackDamageValue = new("攻击伤害");

        public BoxingGlovesShopItem()
        {
            attackDamageValue.Initialize(this);
        }

        public int AttackDamage => attackDamageValue;

        public void SetAttackDamage(int newAttackDamage)
        {
            attackDamageValue.SetBaseValue(Mathf.Max(0, newAttackDamage));
        }

        protected override void OnTemplateSet()
        {
            base.OnTemplateSet();
            // 从格斗拳套商店道具模板中读取两次攻击伤害
            if (template is WeaponTemplate boxingGlovesTemplate)
            {
                attackDamageValue.SetBaseValue(boxingGlovesTemplate.weaponDamage);
            }
        }

        protected override string FormatDescriptionInternal(string formattedDescription)
        {
            return formattedDescription.Replace("{damage}", AttackDamage.ToString());
        }
    }
}