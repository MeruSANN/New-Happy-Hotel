using Sirenix.Serialization;

namespace HappyHotel.Intent.Settings
{
    // Setting：为 IncreaseAttackPowerIntent 提供数值
    [System.Serializable]
    [IntentSettingFor("IncreaseAttackPower")]
    public class IncreaseAttackPowerSetting : IIntentSetting
    {
        [OdinSerialize]
        public int amount = 1;

        public void ConfigureIntent(IntentBase intent)
        {
            var typed = intent as IncreaseAttackPowerIntent;
            if (typed == null) return;
            typed.SetAmount(amount);
        }
    }
}


