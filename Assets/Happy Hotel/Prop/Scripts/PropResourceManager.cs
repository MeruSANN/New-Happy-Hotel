using HappyHotel.Core.Registry;
using HappyHotel.Equipment.Templates;
using HappyHotel.Prop.Factories;
using HappyHotel.Prop.Settings;
using UnityEngine;

namespace HappyHotel.Prop
{
    public class
        PropResourceManager : ResourceManagerBase<PropBase, PropTypeId, IPropFactory, ItemTemplate, IPropSetting>
    {
        protected override void LoadTypeResources(PropTypeId type)
        {
            var descriptor = (registry as PropRegistry)!.GetDescriptor(type);

            var template = Resources.Load<ItemTemplate>(descriptor.TemplatePath);
            if (template != null)
                templateCache[descriptor.Type] = template;
            else
                Debug.LogWarning($"无法加载道具模板: {descriptor.TemplatePath}");
        }
    }
}