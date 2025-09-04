namespace HappyHotel.Equipment
{
    // 格斗拳套+（与基础版逻辑一致）
    public class BoxingGlovesPlus : EquipmentBase
    {
        private readonly Core.ValueProcessing.EquipmentValue attackDamageValue = new("攻击伤害");

        public BoxingGlovesPlus()
        {
            attackDamageValue.Initialize(this);
        }

        public int AttackDamage => attackDamageValue?.GetFinalValue() ?? 0;

        public void SetAttackDamage(int newFirstAttackDamage)
        {
            attackDamageValue.SetBaseValue(UnityEngine.Mathf.Max(0, newFirstAttackDamage));
        }

        protected override void OnTemplateSet()
        {
            base.OnTemplateSet();

            if (Template is Equipment.Templates.WeaponTemplate t)
            {
                attackDamageValue.SetBaseValue(t.weaponDamage);
            }
        }

        protected override string FormatDescriptionInternal(string formattedDescription)
        {
            return formattedDescription.Replace("{damage}", AttackDamage.ToString());
        }
    }
}