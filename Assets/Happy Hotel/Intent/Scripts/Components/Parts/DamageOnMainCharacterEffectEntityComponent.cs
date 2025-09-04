using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.EntityComponent;
using HappyHotel.Core.ValueProcessing.Components;
using IEventListener = HappyHotel.Core.EntityComponent.IEventListener;

namespace HappyHotel.Intent.Components.Parts
{
	// 纯效果：对主角结算一次伤害（不播放弹射物）
	public class DamageOnMainCharacterEffectEntityComponent : EntityComponentBase, IEventListener
	{
		public void OnEvent(EntityComponentEvent evt)
		{
			if (evt.EventName != "ApplyOnTarget") return;
			var data = evt.Data as ApplyOnTargetEventData;
			if (data == null || data.Target == null) return;

			var host = GetHost() as IntentBase;
			var owner = host?.Owner;
			if (owner == null) return;

			var ap = owner.GetBehaviorComponent<AttackPowerComponent>();
			if (ap == null) return;

			var damage = ap.GetAttackPower();
			if (damage <= 0) return;

			var targetHp = data.TargetHp ?? data.Target.GetBehaviorComponent<HitPointValueComponent>();
			if (targetHp == null) return;
			targetHp.TakeDamage(damage, HappyHotel.Core.ValueProcessing.DamageSourceType.Attack, owner);
		}
	}
}


