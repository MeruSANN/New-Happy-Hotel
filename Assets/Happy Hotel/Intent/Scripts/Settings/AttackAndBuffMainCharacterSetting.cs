using HappyHotel.Buff;
using HappyHotel.Buff.Settings;
using HappyHotel.Core.Registry;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

namespace HappyHotel.Intent.Settings
{
	// Setting：为 AttackAndBuffMainCharacterIntent 配置 Buff 类型与参数
	[System.Serializable]
	[IntentSettingFor("AttackAndBuffMainCharacter")]
	public class AttackAndBuffMainCharacterSetting : IIntentSetting
	{
		[OdinSerialize, ValueDropdown("GetAllBuffTypeIds"), OnValueChanged("OnBuffTypeChanged")]
		private string buffType = "";

		[OdinSerialize, InlineProperty, HideLabel]
		[HideReferenceObjectPicker]
		private IBuffSetting buffSetting;

		public void ConfigureIntent(IntentBase intent)
		{
			var typed = intent as AttackAndBuffMainCharacterIntent;
			if (typed == null) return;
			EnsureSettingInstance();
			typed.SetBuffToApply(buffType, buffSetting);
		}

		private void EnsureSettingInstance()
		{
			if (string.IsNullOrEmpty(buffType)) { buffSetting = null; return; }
			var t = BuffSettingTypeLookup.GetSettingTypeFor(buffType);
			if (t == null) { buffSetting = null; return; }
			if (buffSetting == null || buffSetting.GetType() != t)
				buffSetting = (IBuffSetting)System.Activator.CreateInstance(t);
		}

		private void OnBuffTypeChanged()
		{
			EnsureSettingInstance();
		}

		private static System.Collections.Generic.IEnumerable<string> GetAllBuffTypeIds()
		{
			return RegistryTypeIdUtility.GetRegisteredTypeIdsByRegistry<BuffRegistry>();
		}
	}
}


