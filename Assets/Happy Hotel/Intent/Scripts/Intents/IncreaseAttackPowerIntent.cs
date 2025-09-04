using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.EntityComponent;
using HappyHotel.Core.ValueProcessing.Components;
using UnityEngine;
using Cysharp.Threading.Tasks;
using HappyHotel.Intent.Components.Parts;

namespace HappyHotel.Intent
{
	// 意图：增加敌人攻击力（通过组件响应Execute事件）
	[AutoInitEntityComponent(typeof(IncreaseAttackPowerEntityComponent))]
	public class IncreaseAttackPowerIntent : IntentBase
	{
		private int amount;

		public void SetAmount(int value)
		{
			amount = Mathf.Max(0, value);
			var comp = GetEntityComponent<IncreaseAttackPowerEntityComponent>();
			comp?.SetAmount(amount);
		}

		public override string GetDisplayValue()
		{
			return amount.ToString();
		}

		public override UniTask ExecuteAsync()
		{
			return base.ExecuteAsync();
		}
	}
}


