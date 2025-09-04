using HappyHotel.Action.Components.Parts;

namespace HappyHotel.Action.Settings
{
    public class BlockActionSetting : IActionSetting
    {
        private readonly int blockAmount;

        public BlockActionSetting(int blockAmount)
        {
            this.blockAmount = blockAmount;
        }

        public void ConfigureAction(ActionBase action)
        {
            if (action is BlockAction blockAction)
            {
                var blockComponent = blockAction.GetEntityComponent<BlockEntityComponent>();
                if (blockComponent == null) blockComponent = blockAction.AddEntityComponent<BlockEntityComponent>();
                blockComponent.SetBlockAmount(blockAmount);
            }
        }
    }
}