using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.EntityComponent;
using HappyHotel.Core.ValueProcessing.Components;
using Cysharp.Threading.Tasks;
using HappyHotel.Intent.Components.Parts;

namespace HappyHotel.Intent
{
	// 意图：攻击主角色并获得护甲（统筹发射+伤害效果+自护甲）
	[AutoInitEntityComponent(typeof(ProjectileEmitterEntityComponent))]
	[AutoInitEntityComponent(typeof(DamageOnMainCharacterEffectEntityComponent))]
	[AutoInitEntityComponent(typeof(SelfArmorEntityComponent))]
	public class AttackMainCharacterAndGainArmorIntent : IntentBase
	{
		private int armorAmount;

		public void SetArmorAmount(int value)
		{
			armorAmount = value < 0 ? 0 : value;
			var armorComp = GetEntityComponent<SelfArmorEntityComponent>();
			armorComp?.SetAmount(armorAmount);
		}

		protected override void OnTemplateSet()
		{
			GetEntityComponent<ProjectileEmitterEntityComponent>()?.RefreshFromTemplate();
		}

		public override string GetDisplayValue()
		{
			if (Owner == null) return "";
			var ap = Owner.GetBehaviorComponent<AttackPowerComponent>();
			if (ap == null) return "";
			return $"{ap.GetAttackPower().ToString()}({armorAmount})";
		}
	}
}


