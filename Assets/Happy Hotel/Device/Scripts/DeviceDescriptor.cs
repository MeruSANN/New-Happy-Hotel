namespace HappyHotel.Device
{
    public class DeviceDescriptor
    {
        public DeviceDescriptor(DeviceTypeId type, string templatePath)
        {
            Type = type;
            TemplatePath = templatePath;
        }

        public DeviceTypeId Type { get; }
        public string TemplatePath { get; }
    }
}