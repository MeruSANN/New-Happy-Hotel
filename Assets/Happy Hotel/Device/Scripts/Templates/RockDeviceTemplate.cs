using UnityEngine;

namespace HappyHotel.Device.Templates
{
    // 石头装置配置
    [CreateAssetMenu(fileName = "Rock Device Template", menuName = "Happy Hotel/Devices/Rock Device Template")]
    public class RockDeviceTemplate : DeviceTemplate
    {
        [Header("石头属性")] public bool isDestructible;
    }
}