namespace HappyHotel.Equipment
{
    // 轻甲+（与基础版逻辑一致）
    public class LightArmorPlus : EquipmentBase
    {
        private readonly Core.ValueProcessing.EquipmentValue armorAmountValue = new("护甲值");

        public LightArmorPlus()
        {
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

            if (Template is Equipment.Templates.ArmorTemplate armorTemplate)
            {
                armorAmountValue.SetBaseValue(armorTemplate.armorAmount);
            }
        }

        protected override string FormatDescriptionInternal(string formattedDescription)
        {
            return formattedDescription.Replace("{armor}", ArmorAmount.ToString());
        }
    }
}