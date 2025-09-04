using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.EntityComponent;
using HappyHotel.Core.ValueProcessing.Components;
using Cysharp.Threading.Tasks;
using HappyHotel.Intent.Components.Parts;

namespace HappyHotel.Intent
{
	// 意图：多次攻击主角（统筹多段发射+纯效果）
	[AutoInitEntityComponent(typeof(MultiProjectileEmitterEntityComponent))]
	[AutoInitEntityComponent(typeof(DamageOnMainCharacterEffectEntityComponent))]
	public class MultiAttackMainCharacterIntent : IntentBase
	{
		private int attackCount = 1;
		private float intervalSeconds = 0.2f;

		public void SetAttackCount(int value)
		{
			attackCount = value < 1 ? 1 : value;
			GetEntityComponent<MultiProjectileEmitterEntityComponent>()?.SetAttackCount(attackCount);
		}

		public void SetIntervalSeconds(float value)
		{
			intervalSeconds = value < 0f ? 0f : value;
			GetEntityComponent<MultiProjectileEmitterEntityComponent>()?.SetIntervalSeconds(intervalSeconds);
		}

		protected override void OnTemplateSet()
		{
			GetEntityComponent<MultiProjectileEmitterEntityComponent>()?.RefreshFromTemplate();
		}

		public override string GetDisplayValue()
		{
			if (Owner == null) return "";
			var ap = Owner.GetBehaviorComponent<AttackPowerComponent>();
			if (ap == null) return "";
			var dmg = ap.GetAttackPower();
			return $"{dmg}×{attackCount}";
		}

		public override UniTask ExecuteAsync()
		{
			return base.ExecuteAsync();
		}
	}
}
