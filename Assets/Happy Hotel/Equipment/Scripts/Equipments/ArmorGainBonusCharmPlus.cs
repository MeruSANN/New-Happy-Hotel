namespace HappyHotel.Equipment
{
    // 护甲获得增益护符+（与基础版逻辑一致）
    public class ArmorGainBonusCharmPlus : EquipmentBase
    {
        private readonly Core.ValueProcessing.EquipmentValue bonusStacksValue = new("护甲额外获得层数");

        public ArmorGainBonusCharmPlus()
        {
            bonusStacksValue.Initialize(this);
        }

        public int BonusStacks => bonusStacksValue?.GetFinalValue() ?? 0;

        protected override void OnTemplateSet()
        {
            base.OnTemplateSet();
            if (Template is Equipment.Templates.ArmorGainBonusCharmTemplate t)
            {
                bonusStacksValue.SetBaseValue(UnityEngine.Mathf.Max(1, t.bonusStacks));
            }
        }

        protected override string FormatDescriptionInternal(string formattedDescription)
        {
            return formattedDescription.Replace("{stacks}", BonusStacks.ToString());
        }
    }
}