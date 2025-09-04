using HappyHotel.Core.ValueProcessing;
using HappyHotel.Equipment.Templates;

namespace HappyHotel.Equipment
{
    // 铁剑武器实现
    public class IronSword : EquipmentBase
    {
        // 使用ProcessableValue替代原来的int字段
        private readonly EquipmentValue damageValue = new("武器伤害");

        public IronSword()
        {
            // 初始化数值
            damageValue.Initialize(this);
        }

        // 公开属性返回处理后的最终数值
        public int Damage => damageValue?.GetFinalValue() ?? 0;

        public void SetDamage(int newDamage)
        {
            damageValue.SetBaseValue(newDamage);
        }

        public int GetDamage()
        {
            return Damage;
        }

        protected override void OnTemplateSet()
        {
            base.OnTemplateSet();

            if (Template is WeaponTemplate weaponTemplate) SetDamage(weaponTemplate.weaponDamage);
        }

        protected override string FormatDescriptionInternal(string formattedDescription)
        {
            // 为铁剑添加特定的占位符替换，显示最终数值（包含增强效果）
            return formattedDescription
                .Replace("{damage}", Damage.ToString());
        }
    }
}