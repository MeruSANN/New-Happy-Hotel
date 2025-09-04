using HappyHotel.Action.Components.Parts;

namespace HappyHotel.Action.Settings
{
    public class ArmorActionSetting : IActionSetting
    {
        private readonly int armorAmount;

        public ArmorActionSetting(int armorAmount)
        {
            this.armorAmount = armorAmount;
        }

        public void ConfigureAction(ActionBase action)
        {
            if (action is ArmorAction armorAction)
            {
                var armorComponent = armorAction.GetEntityComponent<ArmorEntityComponent>();
                if (armorComponent == null) armorComponent = armorAction.AddEntityComponent<ArmorEntityComponent>();
                armorComponent.SetArmorAmount(armorAmount);
            }
        }
    }
}