using System;
using System.Collections.Generic;
using System.Linq;
using HappyHotel.Core.Registry;
using HappyHotel.Device.Factories;
using HappyHotel.Device.Settings;
using HappyHotel.Device.Templates;

namespace HappyHotel.Device
{
    public class DeviceRegistry : RegistryBase<DeviceBase, DeviceTypeId, IDeviceFactory, DeviceTemplate, IDeviceSetting>
    {
        private readonly Dictionary<DeviceTypeId, DeviceDescriptor> descriptors = new();

        protected override void OnRegister(RegistrationAttribute attr)
        {
            var type = GetType(attr.TypeId);
            descriptors[type] = new DeviceDescriptor(type, attr.TemplatePath);
        }

        protected override Type GetRegistrationAttributeType()
        {
            return typeof(DeviceRegistrationAttribute);
        }

        public List<DeviceDescriptor> GetAllDescriptors()
        {
            return descriptors.Values.ToList();
        }

        public DeviceDescriptor GetDescriptor(DeviceTypeId id)
        {
            return descriptors[id];
        }

        #region Singleton

        private static DeviceRegistry instance;

        public static DeviceRegistry Instance
        {
            get
            {
                if (instance == null) instance = new DeviceRegistry();
                return instance;
            }
        }

        #endregion
    }
}