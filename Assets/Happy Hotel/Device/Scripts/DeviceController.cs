using System.Collections.Generic;
using HappyHotel.Core.Grid;
using HappyHotel.Core.Grid.Components;
using HappyHotel.Core.Singleton;
using HappyHotel.Device.Settings;
using UnityEngine;

namespace HappyHotel.Device
{
    // 装置控制器，负责装置的放置、移除和管理业务逻辑
    [ManagedSingleton(SceneLoadMode.Exclude, "ShopScene", "MainMenu")]
    [SingletonInitializationDependency(typeof(GridObjectManager))]
    public class DeviceController : SingletonBase<DeviceController>
    {
        protected override void OnSingletonAwake()
        {
            // 确保GridObjectManager已初始化
            if (!GridObjectManager.Instance) Debug.LogError("GridObjectManager未初始化，装置控制可能无法正常工作");
        }

        public DeviceBase PlaceDevice(Vector2Int position, DeviceTypeId deviceType, IDeviceSetting setting = null)
        {
            if (!GridObjectManager.Instance)
            {
                Debug.LogError("GridObjectManager未初始化，无法放置装置");
                return null;
            }

            // 使用DeviceManager创建装置
            var device = DeviceManager.Instance.Create(deviceType, setting);
            if (device)
            {
                // 设置装置位置
                device.transform.parent = transform;
                device.GetBehaviorComponent<GridObjectComponent>().MoveTo(position);
            }

            return device;
        }

        public void RemoveDevice(Vector2Int position)
        {
            if (!GridObjectManager.Instance)
            {
                Debug.LogError("GridObjectManager未初始化，无法移除装置");
                return;
            }

            // 获取该位置的所有装置
            var devices = GridObjectManager.Instance.GetObjectsOfTypeAt<DeviceBase>(position);

            // 移除所有装置
            foreach (var device in devices)
            {
                DeviceManager.Instance.Remove(device);
                Destroy(device.gameObject);
                Debug.Log($"已移除位置 {position} 的装置");
            }
        }

        public List<DeviceBase> GetDevicesAt(Vector2Int position)
        {
            if (!GridObjectManager.Instance)
            {
                Debug.LogError("GridObjectManager未初始化，无法获取装置");
                return new List<DeviceBase>();
            }

            return GridObjectManager.Instance.GetObjectsOfTypeAt<DeviceBase>(position);
        }

        public List<DeviceBase> GetAllDevices()
        {
            if (!GridObjectManager.Instance)
            {
                Debug.LogError("GridObjectManager未初始化，无法获取装置");
                return new List<DeviceBase>();
            }

            return GridObjectManager.Instance.GetObjectsOfType<DeviceBase>();
        }

        // 清除所有装置
        public void ClearAllDevices()
        {
            if (!GridObjectManager.Instance)
            {
                Debug.LogError("GridObjectManager未初始化，无法清除装置");
                return;
            }

            var devices = GridObjectManager.Instance.GetObjectsOfType<DeviceBase>();
            foreach (var device in devices)
            {
                DeviceManager.Instance.Remove(device);
                Destroy(device.gameObject);
            }

            Debug.Log("已清除所有装置");
        }
    }
}