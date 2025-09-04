using HappyHotel.Core.Registry;
using HappyHotel.Intent.Factories;
using HappyHotel.Intent.Settings;
using HappyHotel.Intent.Templates;
using UnityEngine;

namespace HappyHotel.Intent
{
	public class IntentResourceManager : ResourceManagerBase<IntentBase, IntentTypeId, IIntentFactory, IntentTemplate, IIntentSetting>
	{
		protected override void LoadTypeResources(IntentTypeId type)
		{
			var descriptor = (registry as IntentRegistry)!.GetDescriptor(type);
			var template = Resources.Load<IntentTemplate>(descriptor.TemplatePath);
			if (template)
				templateCache[descriptor.Type] = template;
			else
				Debug.LogWarning($"无法加载意图模板: {descriptor.TemplatePath}");
		}
	}
}


