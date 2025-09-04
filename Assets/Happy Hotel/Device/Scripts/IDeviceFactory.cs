using HappyHotel.Core.Registry;
using HappyHotel.Device.Settings;
using HappyHotel.Device.Templates;

namespace HappyHotel.Device.Factories
{
    public interface IDeviceFactory : IFactory<DeviceBase, DeviceTemplate, IDeviceSetting>
    {
    }
}