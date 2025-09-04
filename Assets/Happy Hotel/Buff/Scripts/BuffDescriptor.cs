namespace HappyHotel.Buff
{
    // Buff描述符
    public class BuffDescriptor
    {
        public BuffDescriptor(BuffTypeId typeId, string templatePath)
        {
            TypeId = typeId;
            TemplatePath = templatePath;
        }

        public BuffTypeId TypeId { get; private set; }
        public string TemplatePath { get; private set; }
    }
}