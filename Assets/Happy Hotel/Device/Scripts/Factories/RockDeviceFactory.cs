using UnityEngine;

namespace HappyHotel.Device.Factories
{
    [DeviceRegistration(
        "Rock",
        "Templates/Rock Device Template")]
    public class RockDeviceFactory : DeviceFactoryBase<RockDevice>
    {
    }
}