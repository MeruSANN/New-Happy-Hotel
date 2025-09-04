using HappyHotel.Core.ValueProcessing;
using HappyHotel.Equipment.Templates;

namespace HappyHotel.Equipment
{
    // 触发增幅护符装备
    public class PropBoostCharm : EquipmentBase
    {
        private readonly EquipmentValue buffBonusValue = new("触发增幅");

        public PropBoostCharm()
        {
            // 初始化数值
            buffBonusValue.Initialize(this);
        }

        public int BuffBonus => buffBonusValue;

        public void SetBuffBonus(int value)
        {
            buffBonusValue.SetBaseValue(value);
        }

        public int GetBuffBonus()
        {
            return buffBonusValue;
        }

        protected override void OnTemplateSet()
        {
            base.OnTemplateSet();

            if (Template is PropBoostCharmTemplate t)
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
