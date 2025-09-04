using System;
using Cysharp.Threading.Tasks;
using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.EntityComponent;
using HappyHotel.Core.ValueProcessing.Components;
using HappyHotel.Intent.Templates;
using UnityEngine;
using HappyHotel.Intent.Utilities;
using IEventListener = HappyHotel.Core.EntityComponent.IEventListener;
using Object = UnityEngine.Object;

namespace HappyHotel.Intent.Components.Parts
{
	// 多次攻击主角的组件，支持固定间隔依次发射弹射物
	public class MultiAttackMainCharacterEntityComponent : EntityComponentBase, IEventListener
	{
		private Sprite cachedProjectileSprite;
		private float cachedAnimationDuration = 1f;
		private float cachedProjectileSpeed = 5f;
		private int attackCount = 1;
		private float intervalSeconds = 0.2f;
		private IntentBase intentHost;

		public override void OnAttach(EntityComponentContainer host)
		{
			base.OnAttach(host);
			intentHost = host as IntentBase;
			CacheFromTemplate();
		}

		private void CacheFromTemplate()
		{
			var t = intentHost?.Template as ProjectileIntentTemplate;
			if (t == null) return;
			cachedProjectileSprite = t.projectileSprite;
			cachedAnimationDuration = Mathf.Max(0.1f, t.animationDuration);
			cachedProjectileSpeed = Mathf.Max(0.1f, t.projectileSpeed);
		}

		public void RefreshFromTemplate()
		{
			CacheFromTemplate();
		}

		public void SetAttackCount(int value)
		{
			attackCount = value < 1 ? 1 : value;
		}

		public void SetIntervalSeconds(float value)
		{
			intervalSeconds = value < 0f ? 0f : value;
		}

		public void OnEvent(EntityComponentEvent evt)
		{
			if (evt.EventName != "Execute") return;
			PerformAttacksAsync().Forget();
		}

		private async UniTask PerformAttacksAsync()
		{
			if (intentHost == null || intentHost.Owner == null) return;

			var attackPower = intentHost.Owner.GetBehaviorComponent<AttackPowerComponent>();
			if (attackPower == null) return;

			var mainCharacter = GameObject.FindGameObjectWithTag("MainCharacter");
			if (mainCharacter == null) return;

			var behaviorContainer = mainCharacter.GetComponent<BehaviorComponentContainer>();
			if (behaviorContainer == null) return;

			var hp = behaviorContainer.GetBehaviorComponent<HitPointValueComponent>();
			if (hp == null) return;

			for (var i = 0; i < attackCount; i++)
			{
				var damage = attackPower.GetAttackPower();
				if (damage > 0)
				{
					if (cachedProjectileSprite == null)
					{
						hp.TakeDamage(damage, HappyHotel.Core.ValueProcessing.DamageSourceType.Attack, intentHost.Owner);
						RaiseAttackEvent(mainCharacter, damage, i, i == attackCount - 1);
					}
					else
					{
						await PlayProjectileAndDealDamageAsync(mainCharacter, hp, damage, i, i == attackCount - 1);
					}
				}

				if (i < attackCount - 1 && intervalSeconds > 0f)
					await UniTask.Delay(TimeSpan.FromSeconds(intervalSeconds));
			}
		}

		private void RaiseAttackEvent(GameObject target, int damage, int hitIndex, bool isLast)
		{
			var hub = intentHost.Owner.GetBehaviorComponent<HappyHotel.Core.Combat.AttackEventHub>() ?? intentHost.Owner.AddBehaviorComponent<HappyHotel.Core.Combat.AttackEventHub>();
			if (hub != null)
			{
				hub.RaiseAfterDealDamage(new HappyHotel.Core.Combat.AttackEventData
				{
					Attacker = intentHost.Owner,
					Target = target.GetComponent<BehaviorComponentContainer>(),
					BaseDamage = damage,
					FinalDamage = damage,
					SourceType = HappyHotel.Core.ValueProcessing.DamageSourceType.Attack,
					HitIndex = hitIndex,
					IsLastHitOfAction = isLast
				});
			}
		}

		private async UniTask PlayProjectileAndDealDamageAsync(GameObject target, HitPointValueComponent targetHp, int damage, int hitIndex, bool isLast)
		{
			var startPosition = intentHost.Owner.transform.position;
			var endPosition = target.transform.position;
			await ProjectileUtility.Play(cachedProjectileSprite, startPosition, endPosition,
				cachedProjectileSpeed, cachedAnimationDuration);
			targetHp.TakeDamage(damage, HappyHotel.Core.ValueProcessing.DamageSourceType.Attack, intentHost.Owner);
			RaiseAttackEvent(target, damage, hitIndex, isLast);
		}
	}
}


