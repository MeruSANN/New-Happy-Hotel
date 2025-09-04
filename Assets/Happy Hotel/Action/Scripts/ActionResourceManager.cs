using HappyHotel.Action.Factories;
using HappyHotel.Action.Settings;
using HappyHotel.Action.Templates;
using HappyHotel.Core.Registry;
using UnityEngine;

namespace HappyHotel.Action
{
    public class ActionResourceManager : ResourceManagerBase<ActionBase, ActionTypeId, IActionFactory, ActionTemplate,
        IActionSetting>
    {
        protected override void LoadTypeResources(ActionTypeId type)
        {
            var descriptor = (registry as ActionRegistry)!.GetDescriptor(type);

            var template = Resources.Load<ActionTemplate>(descriptor.TemplatePath);
            if (template)
                templateCache[descriptor.Type] = template;
            else
                Debug.LogWarning($"无法加载道具模板: {descriptor.TemplatePath}");
        }
    }
}