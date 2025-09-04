namespace HappyHotel.Prop
{
    public class PropDescriptor
    {
        public PropDescriptor(PropTypeId type, string templatePath)
        {
            Type = type;
            TemplatePath = templatePath;
        }

        public PropTypeId Type { get; }
        public string TemplatePath { get; }
    }
}