using HappyHotel.Core.ValueProcessing;
using HappyHotel.Equipment.Templates;
using UnityEngine;

namespace HappyHotel.Shop
{
    // 格斗拳套+ 商店物品
    public class BoxingGlovesPlusShopItem : EquipmentShopItemBase
    {
        private EquipmentValue attackDamageValue = new("攻击伤害");

        public BoxingGlovesPlusShopItem()
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