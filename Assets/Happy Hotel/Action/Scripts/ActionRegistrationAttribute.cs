using HappyHotel.Core.Registry;

namespace HappyHotel.Action
{
    public class ActionRegistrationAttribute : RegistrationAttribute
    {
        public ActionRegistrationAttribute(string typeId, string templatePath = null) : base(typeId, templatePath)
        {
        }
    }
}