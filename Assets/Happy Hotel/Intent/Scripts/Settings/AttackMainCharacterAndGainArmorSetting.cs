using Sirenix.Serialization;

namespace HappyHotel.Intent.Settings
{
	// Setting：为 AttackMainCharacterAndGainArmorIntent 提供护甲数值
	[System.Serializable]
	[IntentSettingFor("AttackMainCharacterAndGainArmor")]
	public class AttackMainCharacterAndGainArmorSetting : IIntentSetting
	{
		[OdinSerialize]
		public int armorAmount = 1;

		public void ConfigureIntent(IntentBase intent)
		{
			var typed = intent as AttackMainCharacterAndGainArmorIntent;
			if (typed == null) return;
			typed.SetArmorAmount(armorAmount);
		}
	}
}


