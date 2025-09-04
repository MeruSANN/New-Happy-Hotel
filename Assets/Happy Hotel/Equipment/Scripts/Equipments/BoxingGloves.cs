using HappyHotel.Core.ValueProcessing;
using HappyHotel.Equipment.Templates;
using UnityEngine;

namespace HappyHotel.Equipment
{
    // 格斗拳套装备实现
    public class BoxingGloves : EquipmentBase
    {
        // 使用ProcessableValue替代原来的int字段
        private readonly EquipmentValue attackDamageValue = new("攻击伤害");

        public BoxingGloves()
        {
            // 初始化数值
            attackDamageValue.Initialize(this);
        }

        // 公开属性返回处理后的最终数值
        public int AttackDamage => attackDamageValue?.GetFinalValue() ?? 0;

        public void SetAttackDamage(int newFirstAttackDamage)
        {
            attackDamageValue.SetBaseValue(Mathf.Max(0, newFirstAttackDamage));
        }

        protected override void OnTemplateSet()
        {
            base.OnTemplateSet();

            if (Template is WeaponTemplate boxingGlovesTemplate)
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