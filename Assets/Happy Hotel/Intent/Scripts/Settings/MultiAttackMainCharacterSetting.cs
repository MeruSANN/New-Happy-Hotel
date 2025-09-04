using Sirenix.Serialization;

namespace HappyHotel.Intent.Settings
{
	// Setting：配置多次攻击的次数与发射间隔
	[System.Serializable]
	[IntentSettingFor("MultiAttackMainCharacter")]
	public class MultiAttackMainCharacterSetting : IIntentSetting
	{
		[OdinSerialize]
		public int attackCount = 2;

		[OdinSerialize]
		public float intervalSeconds = 0.2f;

		public void ConfigureIntent(IntentBase intent)
		{
			var typed = intent as MultiAttackMainCharacterIntent;
			if (typed == null) return;
			typed.SetAttackCount(attackCount);
			typed.SetIntervalSeconds(intervalSeconds);
		}
	}
}


