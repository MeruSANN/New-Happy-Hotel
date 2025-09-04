namespace HappyHotel.Equipment
{
    // 费用上限饰品+（与基础版逻辑一致）
    public class CostCapCharmPlus : EquipmentBase
    {
        private readonly Core.ValueProcessing.EquipmentValue maxCostBonusValue = new("费用上限加成");

        public CostCapCharmPlus()
        {
            maxCostBonusValue.Initialize(this);
        }

        public int MaxCostBonus => maxCostBonusValue?.GetFinalValue() ?? 0;

        public void SetMaxCostBonus(int value)
        {
            maxCostBonusValue.SetBaseValue(UnityEngine.Mathf.Max(0, value));
        }

        protected override void OnTemplateSet()
        {
            base.OnTemplateSet();

            if (Template is Equipment.Templates.CostCapCharmTemplate t)
            {
                maxCostBonusValue.SetBaseValue(UnityEngine.Mathf.Max(0, t.maxCostBonus));
            }
        }

        protected override string FormatDescriptionInternal(string formattedDescription)
        {
            return formattedDescription.Replace("{maxCostBonus}", MaxCostBonus.ToString());
        }
    }
}