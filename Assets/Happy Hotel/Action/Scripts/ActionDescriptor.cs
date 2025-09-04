namespace HappyHotel.Action
{
    public class ActionDescriptor
    {
        public ActionDescriptor(ActionTypeId type, string templatePath)
        {
            Type = type;
            TemplatePath = templatePath;
        }

        public ActionTypeId Type { get; }
        public string TemplatePath { get; }
    }
}