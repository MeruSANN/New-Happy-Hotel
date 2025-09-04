namespace HappyHotel.Equipment
{
    // 战之护符+（与基础版逻辑一致）
    public class WarCharmPlus : EquipmentBase
    {
        private readonly Core.ValueProcessing.AttackEquipmentValue attackDamageValue = new("攻击伤害");
        private readonly Core.ValueProcessing.EquipmentValue armorAmountValue = new("护甲值");

        public WarCharmPlus()
        {
            attackDamageValue.Initialize(this);
            armorAmountValue.Initialize(this);
        }

        public int AttackDamage => attackDamageValue?.GetFinalValue() ?? 0;
        public int ArmorAmount => armorAmountValue?.GetFinalValue() ?? 0;

        protected override void OnTemplateSet()
        {
            base.OnTemplateSet();
            if (Template is Equipment.Templates.WarCharmTemplate t)
            {
                attackDamageValue.SetBaseValue(UnityEngine.Mathf.Max(0, t.weaponDamage));
                armorAmountValue.SetBaseValue(UnityEngine.Mathf.Max(0, t.armorAmount));
            }
        }

        protected override string FormatDescriptionInternal(string formattedDescription)
        {
            return formattedDescription
                .Replace("{damage}", AttackDamage.ToString())
                .Replace("{armor}", ArmorAmount.ToString());
        }
    }
}