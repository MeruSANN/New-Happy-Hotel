using Sirenix.OdinInspector;
using UnityEngine;

namespace HappyHotel.Device.Templates
{
    // 装置配置的基类
    [CreateAssetMenu(fileName = "New Device Template", menuName = "Happy Hotel/Devices/Device Template")]
    public class DeviceTemplate : ScriptableObject
    {
        [PreviewField] public Sprite deviceSprite;
    }
}