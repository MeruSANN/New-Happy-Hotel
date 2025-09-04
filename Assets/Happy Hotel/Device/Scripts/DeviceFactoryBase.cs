using System.Reflection;
using HappyHotel.Core.Registry;
using HappyHotel.Device.Settings;
using HappyHotel.Device.Templates;
using UnityEngine;

namespace HappyHotel.Device.Factories
{
    // Device工厂基类，提供自动TypeId设置功能
    public abstract class DeviceFactoryBase<TDevice> : IDeviceFactory
        where TDevice : DeviceBase
    {
        public DeviceBase Create(DeviceTemplate template, IDeviceSetting setting = null)
        {
            var deviceObject = new GameObject(GetDeviceName());
            deviceObject.AddComponent<SpriteRenderer>();

            var device = deviceObject.AddComponent<TDevice>();

            // 自动设置TypeId
            AutoSetTypeId(device);

            if (template) device.SetTemplate(template);
            setting?.ConfigureDevice(device);

            return device;
        }

        private void AutoSetTypeId(DeviceBase device)
        {
            var attr = GetType().GetCustomAttribute<DeviceRegistrationAttribute>();
            if (attr != null)
            {
                var typeId = TypeId.Create<DeviceTypeId>(attr.TypeId);
                ((ITypeIdSettable<DeviceTypeId>)device).SetTypeId(typeId);
            }
        }

        protected virtual string GetDeviceName()
        {
            return typeof(TDevice).Name;
        }
    }
}