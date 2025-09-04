using HappyHotel.Intent.Factories;

namespace HappyHotel.Intent
{
	// 工厂：注册并创建 AttackMainCharacterAndGainArmorIntent
	[IntentRegistration("AttackMainCharacterAndGainArmor",
		"Templates/Attack Main Character And Gain Armor Intent")]
	public class AttackMainCharacterAndGainArmorIntentFactory : IntentFactoryBase<AttackMainCharacterAndGainArmorIntent>
	{
	}
}


