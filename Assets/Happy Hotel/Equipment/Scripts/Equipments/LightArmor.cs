using HappyHotel.Core.ValueProcessing;
using HappyHotel.Equipment.Templates;

namespace HappyHotel.Equipment
{
    // 轻甲装备实现
    public class LightArmor : EquipmentBase
    {
        private readonly EquipmentValue armorAmountValue = new("护甲值");

        public LightArmor()
        {
            // 初始化数值
            armorAmountValue.Initialize(this);
        }

        public int ArmorAmount => armorAmountValue;

        public void SetArmorAmount(int newArmorAmount)
        {
            armorAmountValue.SetBaseValue(newArmorAmount);
        }

        protected override void OnTemplateSet()
        {
            base.OnTemplateSet();

            if (Template is ArmorTemplate armorTemplate) armorAmountValue.SetBaseValue(armorTemplate.armorAmount);
        }

        protected override string FormatDescriptionInternal(string formattedDescription)
        {
            // 为轻甲添加特定的占位符替换
            return formattedDescription
                .Replace("{armor}", ArmorAmount.ToString());
        }
    }
}