using HappyHotel.Core.Registry;

namespace HappyHotel.Buff
{
    // Buff注册属性
    public class BuffRegistrationAttribute : RegistrationAttribute
    {
        public BuffRegistrationAttribute(string typeId, string templatePath = "")
            : base(typeId, templatePath)
        {
        }
    }
}