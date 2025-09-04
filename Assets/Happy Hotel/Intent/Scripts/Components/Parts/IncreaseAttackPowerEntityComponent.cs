using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.EntityComponent;
using HappyHotel.Core.ValueProcessing.Components;
using IEventListener = HappyHotel.Core.EntityComponent.IEventListener;

namespace HappyHotel.Intent.Components.Parts
{
	// 增加自身攻击力的组件
	public class IncreaseAttackPowerEntityComponent : EntityComponentBase, IEventListener
	{
		private int amount;
		private IntentBase intentHost;

		public int Amount => amount;

		public override void OnAttach(EntityComponentContainer host)
		{
			base.OnAttach(host);
			intentHost = host as IntentBase;
		}

		public void SetAmount(int value)
		{
			amount = value < 0 ? 0 : value;
		}

		public void OnEvent(EntityComponentEvent evt)
		{
			if (evt.EventName != "Execute") return;
			if (intentHost == null || intentHost.Owner == null) return;

			var ap = intentHost.Owner.GetBehaviorComponent<AttackPowerComponent>();
			if (ap == null) return;
			if (amount <= 0) return;
			ap.AddAttackPower(amount);
		}
	}
}


