using HappyHotel.Intent.Factories;

namespace HappyHotel.Intent
{
	// 工厂：注册并创建 IncreaseAttackPowerAndGainArmorIntent
	[IntentRegistration("IncreaseAttackPowerAndGainArmor",
		"Templates/Increase Attack Power And Gain Armor Intent")]
	public class IncreaseAttackPowerAndGainArmorIntentFactory : IntentFactoryBase<IncreaseAttackPowerAndGainArmorIntent>
	{
	}
}


