using HappyHotel.Intent.Factories;

namespace HappyHotel.Intent
{
	// 工厂：注册并创建 AttackAndBuffMainCharacterIntent
	[IntentRegistration("AttackAndBuffMainCharacter",
		"Templates/Attack And Buff Main Character Intent")]
	public class AttackAndBuffMainCharacterIntentFactory : IntentFactoryBase<AttackAndBuffMainCharacterIntent>
	{
	}
}


