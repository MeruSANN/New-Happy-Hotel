namespace HappyHotel.Action.Settings
{
    // FollowUpAttackAction的设置类
    public class FollowUpAttackActionSetting : IActionSetting
    {
        public FollowUpAttackActionSetting(int baseDamage, int bonusDamage)
        {
            this.BaseDamage = baseDamage;
            this.BonusDamage = bonusDamage;
        }

        public int BaseDamage { get; }

        public int BonusDamage { get; }

        public void ConfigureAction(ActionBase action)
        {
            if (action is FollowUpAttackAction followUpAction)
            {
                followUpAction.SetBaseDamage(BaseDamage);
                followUpAction.SetBonusDamage(BonusDamage);
            }
        }
    }
}