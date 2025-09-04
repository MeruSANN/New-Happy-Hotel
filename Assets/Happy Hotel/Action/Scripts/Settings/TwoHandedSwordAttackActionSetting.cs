namespace HappyHotel.Action.Settings
{
    public class TwoHandedSwordAttackActionSetting : IActionSetting
    {
        private readonly int baseDamage;
        private readonly int bonusDamage;

        public TwoHandedSwordAttackActionSetting(int baseDamage, int bonusDamage)
        {
            this.baseDamage = baseDamage;
            this.bonusDamage = bonusDamage;
        }

        public void ConfigureAction(ActionBase action)
        {
            if (action is TwoHandedSwordAttackAction twoHandedSwordAttackAction)
            {
                twoHandedSwordAttackAction.SetBaseDamage(baseDamage);
                twoHandedSwordAttackAction.SetBonusDamage(bonusDamage);
            }
        }
    }
}