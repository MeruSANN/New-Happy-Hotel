namespace HappyHotel.Equipment
{
    // 触发增幅护符+（与基础版逻辑一致）
    public class PropBoostCharmPlus : EquipmentBase
    {
        private readonly Core.ValueProcessing.EquipmentValue buffBonusValue = new("触发增幅");

        public PropBoostCharmPlus()
        {
            buffBonusValue.Initialize(this);
        }

        public int BuffBonus => buffBonusValue;

        public void SetBuffBonus(int value)
        {
            buffBonusValue.SetBaseValue(value);
        }

        protected override void OnTemplateSet()
        {
            base.OnTemplateSet();

            if (Template is Equipment.Templates.PropBoostCharmTemplate t)
            {
                buffBonusValue.SetBaseValue(t.buffBonus);
            }
        }

        protected override string FormatDescriptionInternal(string formattedDescription)
        {
            return formattedDescription.Replace("{bonus}", BuffBonus.ToString());
        }
    }
}