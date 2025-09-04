using UnityEngine;

namespace HappyHotel.Device.Templates
{
    // 地刺装置配置
    [CreateAssetMenu(fileName = "Spike Device Template", menuName = "Happy Hotel/Devices/Spike Device Template")]
    public class SpikeDeviceTemplate : DeviceTemplate
    {
        [Header("地刺属性")] public int damage = 1;
    }
}