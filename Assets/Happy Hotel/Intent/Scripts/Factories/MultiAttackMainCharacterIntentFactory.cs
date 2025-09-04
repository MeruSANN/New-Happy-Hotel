using HappyHotel.Intent.Factories;

namespace HappyHotel.Intent
{
	// 工厂：注册并创建 MultiAttackMainCharacterIntent
	[IntentRegistration("MultiAttackMainCharacter",
		"Templates/Multi Attack Main Character Intent")]
	public class MultiAttackMainCharacterIntentFactory : IntentFactoryBase<MultiAttackMainCharacterIntent>
	{
	}
}


