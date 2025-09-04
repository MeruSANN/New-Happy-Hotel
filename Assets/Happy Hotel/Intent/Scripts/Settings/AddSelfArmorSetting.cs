using Sirenix.Serialization;

namespace HappyHotel.Intent.Settings
{
	// Setting：为 AddSelfArmorIntent 提供数值
	[System.Serializable]
	[IntentSettingFor("AddSelfArmor")]
	public class AddSelfArmorSetting : IIntentSetting
	{
		[OdinSerialize]
		public int amount = 1;

		public void ConfigureIntent(IntentBase intent)
		{
			var typed = intent as AddSelfArmorIntent;
			if (typed == null) return;
			typed.SetAmount(amount);
		}
	}
}



