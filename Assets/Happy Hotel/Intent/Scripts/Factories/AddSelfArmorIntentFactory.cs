using HappyHotel.Intent.Factories;

namespace HappyHotel.Intent
{
	// 工厂：注册并创建 AddSelfArmorIntent
	[IntentRegistration("AddSelfArmor",
		"Templates/Add Self Armor Intent")]
	public class AddSelfArmorIntentFactory : IntentFactoryBase<AddSelfArmorIntent>
	{
	}
}



