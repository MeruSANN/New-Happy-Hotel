namespace HappyHotel.Equipment
{
    // 铁剑+武器实现（与基础版逻辑一致）
    public class IronSwordPlus : EquipmentBase
    {
        private readonly Core.ValueProcessing.EquipmentValue damageValue = new("武器伤害");

        public IronSwordPlus()
        {
            damageValue.Initialize(this);
        }

        public int Damage => damageValue?.GetFinalValue() ?? 0;

        public void SetDamage(int newDamage)
        {
            damageValue.SetBaseValue(newDamage);
        }

        protected override void OnTemplateSet()
        {
            base.OnTemplateSet();

            if (Template is Equipment.Templates.WeaponTemplate weaponTemplate)
            {
                damageValue.SetBaseValue(weaponTemplate.weaponDamage);
            }
        }

        protected override string FormatDescriptionInternal(string formattedDescription)
        {
            return formattedDescription.Replace("{damage}", Damage.ToString());
        }
    }
}