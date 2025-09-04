using HappyHotel.Buff.Settings;
using HappyHotel.Core.EntityComponent;
using Cysharp.Threading.Tasks;
using HappyHotel.Buff;
using HappyHotel.Intent.Components.Parts;

namespace HappyHotel.Intent
{
	// 意图：向主角添加Buff（统筹发射+纯效果）
	[AutoInitEntityComponent(typeof(ProjectileEmitterEntityComponent))]
	[AutoInitEntityComponent(typeof(AddBuffOnMainCharacterEffectEntityComponent))]
	public class AddBuffToMainCharacterIntent : IntentBase
	{
		private string buffTypeString;
		private IBuffSetting buffSetting;

		public void SetBuffToApply(string buffType, IBuffSetting setting)
		{
			buffTypeString = buffType;
			buffSetting = setting;
			GetEntityComponent<AddBuffOnMainCharacterEffectEntityComponent>()?.SetBuffToApply(buffTypeString, buffSetting);
		}

		protected override void OnTemplateSet()
		{
			GetEntityComponent<ProjectileEmitterEntityComponent>()?.RefreshFromTemplate();
		}

		public override string GetDisplayValue()
		{
			return buffTypeString ?? "";
		}

		public override UniTask ExecuteAsync()
		{
			return base.ExecuteAsync();
		}
	}
}


