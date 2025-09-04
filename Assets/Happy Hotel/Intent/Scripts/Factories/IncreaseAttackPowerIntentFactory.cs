using HappyHotel.Intent.Factories;

namespace HappyHotel.Intent
{
	// 工厂：注册并创建 IncreaseAttackPowerIntent
	[IntentRegistration("IncreaseAttackPower",
		"Templates/Increase Attack Power Intent")]
	public class IncreaseAttackPowerIntentFactory : IntentFactoryBase<IncreaseAttackPowerIntent>
	{
	}
}


