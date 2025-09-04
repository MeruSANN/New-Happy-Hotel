using Sirenix.Serialization;

namespace HappyHotel.Intent.Settings
{
	// Setting：为 IncreaseAttackPowerAndGainArmorIntent 提供加攻与护甲数值
	[System.Serializable]
	[IntentSettingFor("IncreaseAttackPowerAndGainArmor")]
	public class IncreaseAttackPowerAndGainArmorSetting : IIntentSetting
	{
		[OdinSerialize]
		public int attackAmount = 1;

		[OdinSerialize]
		public int armorAmount = 1;

		public void ConfigureIntent(IntentBase intent)
		{
			var typed = intent as IncreaseAttackPowerAndGainArmorIntent;
			if (typed == null) return;
			typed.SetAttackAmount(attackAmount);
			typed.SetArmorAmount(armorAmount);
		}
	}
}


