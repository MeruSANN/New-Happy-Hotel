namespace HappyHotel.Action.Settings
{
    public class TwoHandedShieldBlockActionSetting : IActionSetting
    {
        private readonly int baseBlock;
        private readonly int bonusBlock;

        public TwoHandedShieldBlockActionSetting(int baseBlock, int bonusBlock)
        {
            this.baseBlock = baseBlock;
            this.bonusBlock = bonusBlock;
        }

        public void ConfigureAction(ActionBase action)
        {
            if (action is TwoHandedShieldBlockAction twoHandedShieldBlockAction)
            {
                twoHandedShieldBlockAction.SetBaseBlock(baseBlock);
                twoHandedShieldBlockAction.SetBonusBlock(bonusBlock);
            }
        }
    }
}