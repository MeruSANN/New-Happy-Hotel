using HappyHotel.Core.Registry;
using HappyHotel.Core.Singleton;
using HappyHotel.Intent.Factories;
using HappyHotel.Intent.Settings;
using HappyHotel.Intent.Templates;

namespace HappyHotel.Intent
{
	[ManagedSingleton(true)]
	public class IntentManager : ManagerBase<IntentManager, IntentBase, IntentTypeId, IIntentFactory, IntentResourceManager, IntentTemplate, IIntentSetting>
	{
		protected override RegistryBase<IntentBase, IntentTypeId, IIntentFactory, IntentTemplate, IIntentSetting> GetRegistry()
		{
			return IntentRegistry.Instance;
		}

		protected override void Initialize()
		{
			base.Initialize();
			isInitialized = true;
		}

		public IntentResourceManager GetResourceManager()
		{
			return resourceManager;
		}
	}
}


