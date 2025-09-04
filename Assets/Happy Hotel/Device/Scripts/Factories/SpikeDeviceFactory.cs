using UnityEngine;

namespace HappyHotel.Device.Factories
{
    [DeviceRegistration(
        "Spike",
        "Templates/Spike Device Template")]
    public class SpikeDeviceFactory : DeviceFactoryBase<SpikeDevice>
    {
    }
}