using HappyHotel.Core;

namespace HappyHotel.Action.Settings
{
    public class ChangeDirectionActionSetting : IActionSetting
    {
        private readonly Direction direction;

        public ChangeDirectionActionSetting(Direction direction)
        {
            this.direction = direction;
        }

        public void ConfigureAction(ActionBase action)
        {
            if (action is ChangeDirectionAction changeDirectionAction) changeDirectionAction.SetDirection(direction);
        }
    }
}