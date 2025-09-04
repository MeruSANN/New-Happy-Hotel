using HappyHotel.Action.Factories;
using HappyHotel.Action.Settings;
using HappyHotel.Action.Templates;
using HappyHotel.Core.Registry;
using HappyHotel.Core.Singleton;

namespace HappyHotel.Action
{
    [ManagedSingleton(true)]
    public class ActionManager : ManagerBase<ActionManager, ActionBase, ActionTypeId, IActionFactory,
        ActionResourceManager, ActionTemplate, IActionSetting>
    {
        protected override RegistryBase<ActionBase, ActionTypeId, IActionFactory, ActionTemplate, IActionSetting>
            GetRegistry()
        {
            return ActionRegistry.Instance;
        }

        protected override void Initialize()
        {
            base.Initialize();

            isInitialized = true;
        }

        public ActionResourceManager GetResourceManager()
        {
            return resourceManager;
        }
    }
}