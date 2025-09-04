using HappyHotel.Core.Registry;
using HappyHotel.Core.Singleton;
using HappyHotel.Device.Factories;
using HappyHotel.Device.Settings;
using HappyHotel.Device.Templates;
using UnityEngine;

namespace HappyHotel.Device
{
    // 装置管理器，负责装置数据加载和创建
    [ManagedSingleton(true)]
    public class DeviceManager : ManagerBase<DeviceManager, DeviceBase, DeviceTypeId, IDeviceFactory,
        DeviceResourceManager, DeviceTemplate, IDeviceSetting>
    {
        protected override RegistryBase<DeviceBase, DeviceTypeId, IDeviceFactory, DeviceTemplate, IDeviceSetting>
            GetRegistry()
        {
            return DeviceRegistry.Instance;
        }

        protected override void Initialize()
        {
            base.Initialize();
            isInitialized = true;
        }

        // 提供一个方便的方法来获取 ResourceManager
        public DeviceResourceManager GetResourceManager()
        {
            if (!isInitialized) Debug.LogWarning("DeviceManager尚未初始化，ResourceManager可能不可用。");
            return resourceManager;
        }
    }
}