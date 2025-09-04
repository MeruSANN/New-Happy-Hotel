using HappyHotel.Core.Registry;
using HappyHotel.Device.Factories;
using HappyHotel.Device.Settings;
using HappyHotel.Device.Templates;
using UnityEngine;

namespace HappyHotel.Device
{
    public class DeviceResourceManager : ResourceManagerBase<DeviceBase, DeviceTypeId, IDeviceFactory, DeviceTemplate,
        IDeviceSetting>
    {
        protected override void LoadTypeResources(DeviceTypeId type)
        {
            var descriptor = (registry as DeviceRegistry)!.GetDescriptor(type);

            var template = Resources.Load<DeviceTemplate>(descriptor.TemplatePath);
            if (template)
                templateCache[descriptor.Type] = template;
            else
                Debug.LogWarning($"无法加载道具模板: {descriptor.TemplatePath}");
        }
    }
}