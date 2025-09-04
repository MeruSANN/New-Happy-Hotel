namespace HappyHotel.Action.Settings
{
    public class FullHealthBonusAttackActionSetting : IActionSetting
    {
        private readonly int baseDamage;
        private readonly int bonusDamage;

        public FullHealthBonusAttackActionSetting(int baseDamage, int bonusDamage)
        {
            this.baseDamage = baseDamage;
            this.bonusDamage = bonusDamage;
        }

        public void ConfigureAction(ActionBase action)
        {
            if (action is FullHealthBonusAttackAction fullHealthBonusAttackAction)
            {
                fullHealthBonusAttackAction.SetBaseDamage(baseDamage);
                fullHealthBonusAttackAction.SetBonusDamage(bonusDamage);
            }
        }
    }
}