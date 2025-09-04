using System.Collections;
using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.EntityComponent;
using HappyHotel.Core.ValueProcessing.Components;
using UnityEngine;
using HappyHotel.Intent.Templates;
using Cysharp.Threading.Tasks;
using HappyHotel.Intent.Components.Parts;

namespace HappyHotel.Intent
{
	// 对主角造成伤害的意图：使用统筹发射+纯效果
	[AutoInitEntityComponent(typeof(ProjectileEmitterEntityComponent))]
	[AutoInitEntityComponent(typeof(DamageOnMainCharacterEffectEntityComponent))]
	public class DamageMainCharacterIntent : IntentBase
	{
		protected override void OnTemplateSet()
		{
			GetEntityComponent<ProjectileEmitterEntityComponent>()?.RefreshFromTemplate();
		}

		public override string GetDisplayValue()
		{
			if (owner == null) return "";
			var attackPower = owner.GetBehaviorComponent<AttackPowerComponent>();
			if (attackPower == null) return "";
			return attackPower.GetAttackPower().ToString();
		}

		public override UniTask ExecuteAsync()
		{
			return base.ExecuteAsync();
		}
	}
}


