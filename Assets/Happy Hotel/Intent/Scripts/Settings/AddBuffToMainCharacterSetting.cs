using HappyHotel.Buff;
using HappyHotel.Buff.Settings;
using HappyHotel.Core.Registry;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

namespace HappyHotel.Intent.Settings
{
	// Setting：配置要对主角施加的Buff类型与参数
	[System.Serializable]
	[IntentSettingFor("AddBuffToMainCharacter")]
	public class AddBuffToMainCharacterSetting : IIntentSetting
	{
		[OdinSerialize, ValueDropdown("GetAllBuffTypeIds"), OnValueChanged("OnBuffTypeChanged")]
		private string buffType = "";

		[OdinSerialize, InlineProperty, HideLabel]
		[HideReferenceObjectPicker]
		private IBuffSetting buffSetting;

		public void ConfigureIntent(IntentBase intent)
		{
			var typed = intent as AddBuffToMainCharacterIntent;
			if (typed == null) return;
			EnsureSettingInstance();
			typed.SetBuffToApply(buffType, buffSetting);
		}

		// 根据选择的BuffType，实例化/切换对应的Setting类型
		private void EnsureSettingInstance()
		{
			if (string.IsNullOrEmpty(buffType)) { buffSetting = null; return; }
			var t = BuffSettingTypeLookup.GetSettingTypeFor(buffType);
			if (t == null) { buffSetting = null; return; }
			if (buffSetting == null || buffSetting.GetType() != t)
				buffSetting = (IBuffSetting)System.Activator.CreateInstance(t);
		}

		// 下拉变化时立刻切换Setting类型
		private void OnBuffTypeChanged()
		{
			EnsureSettingInstance();
		}

		// 供 Odin 下拉使用
		private static System.Collections.Generic.IEnumerable<string> GetAllBuffTypeIds()
		{
			return RegistryTypeIdUtility.GetRegisteredTypeIdsByRegistry<BuffRegistry>();
		}
	}
}


