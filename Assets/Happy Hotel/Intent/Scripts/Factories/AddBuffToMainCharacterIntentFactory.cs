using HappyHotel.Intent.Factories;

namespace HappyHotel.Intent
{
	// 工厂：注册并创建 AddBuffToMainCharacterIntent
	[IntentRegistration("AddBuffToMainCharacter",
		"Templates/Add Buff To Main Character Intent")]
	public class AddBuffToMainCharacterIntentFactory : IntentFactoryBase<AddBuffToMainCharacterIntent>
	{
	}
}


