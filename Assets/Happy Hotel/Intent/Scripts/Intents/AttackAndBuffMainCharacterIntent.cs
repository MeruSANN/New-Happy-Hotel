using HappyHotel.Buff.Settings;
using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.EntityComponent;
using Cysharp.Threading.Tasks;
using HappyHotel.Buff;
using HappyHotel.Core.ValueProcessing.Components;
using HappyHotel.Intent.Components.Parts;

namespace HappyHotel.Intent
{
	// 意图：单次弹射，命中后对主角造成伤害并添加Buff
	[AutoInitEntityComponent(typeof(ProjectileEmitterEntityComponent))]
	[AutoInitEntityComponent(typeof(DamageOnMainCharacterEffectEntityComponent))]
	[AutoInitEntityComponent(typeof(AddBuffOnMainCharacterEffectEntityComponent))]
	public class AttackAndBuffMainCharacterIntent : IntentBase
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
			var ap = Owner?.GetBehaviorComponent<AttackPowerComponent>();
			var dmg = ap != null ? ap.GetAttackPower() : 0;
			return dmg > 0 ? dmg.ToString() : "";
		}

		public override UniTask ExecuteAsync()
		{
			return base.ExecuteAsync();
		}
	}
}


