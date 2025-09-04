using HappyHotel.Core.Registry;

namespace HappyHotel.Intent
{
	// 意图注册特性
	public class IntentRegistrationAttribute : RegistrationAttribute
	{
		public IntentRegistrationAttribute(string typeId, string templatePath = null) : base(typeId, templatePath)
		{
		}
	}
}


