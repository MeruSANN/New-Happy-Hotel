using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.EntityComponent;
using HappyHotel.Core.ValueProcessing.Components;
using HappyHotel.Intent.Templates;
using HappyHotel.Intent.Utilities;
using UnityEngine;
using IEventListener = HappyHotel.Core.EntityComponent.IEventListener;

namespace HappyHotel.Intent.Components.Parts
{
	// 多段发射器：按次数与间隔发射弹射物，命中时派发ApplyOnTarget事件
	public class MultiProjectileEmitterEntityComponent : EntityComponentBase, IEventListener, ICompletesOnApplyOnTarget
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
			if (intentHost?.Template is MultiAttackIntentTemplate mt)
			{
				cachedProjectileSprite = mt.projectileSprite;
				cachedAnimationDuration = Mathf.Max(0.1f, mt.animationDuration);
				cachedProjectileSpeed = Mathf.Max(0.1f, mt.projectileSpeed);
				intervalSeconds = Mathf.Max(0f, mt.intervalSeconds);
				return;
			}
			if (intentHost?.Template is ProjectileIntentTemplate t)
			{
				cachedProjectileSprite = t.projectileSprite;
				cachedAnimationDuration = Mathf.Max(0.1f, t.animationDuration);
				cachedProjectileSpeed = Mathf.Max(0.1f, t.projectileSpeed);
			}
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
			var senderType = evt.Sender != null ? evt.Sender.GetType().Name : "null";
			Debug.Log($"[MultiProjectileEmitter] OnEvent Execute. sender={senderType}, hostNull={(GetHost()==null)}, intentHostNull={(intentHost==null)}");
			EmitAsync().Forget();
		}

		private async UniTask EmitAsync()
		{
			if (intentHost == null || intentHost.Owner == null)
			{
				Debug.LogError("[MultiProjectileEmitter] intentHost or intentHost.Owner is null");
				return;
			}

			var mainCharacter = GameObject.FindGameObjectWithTag("MainCharacter");
			if (mainCharacter == null) return;

			var target = mainCharacter.GetComponent<BehaviorComponentContainer>();
			if (target == null) return;

			var targetHp = target.GetBehaviorComponent<HitPointValueComponent>();

			var launchTasks = new List<UniTask>(attackCount);
			for (var i = 0; i < attackCount; i++)
			{
				var shotIndex = i;
				var shotStart = intentHost.Owner.transform.position;
				var shotEnd = mainCharacter.transform.position;
				var isLast = shotIndex == attackCount - 1;
				async UniTask LaunchOneAsync()
				{
					if (cachedProjectileSprite != null)
						await ProjectileUtility.Play(cachedProjectileSprite, shotStart, shotEnd, cachedProjectileSpeed, cachedAnimationDuration);

					var data = new ApplyOnTargetEventData
					{
						Target = target,
						TargetHp = targetHp,
						HitIndex = shotIndex,
						IsLastHitOfAction = isLast
					};
					var hitEvent = new EntityComponentEvent("ApplyOnTarget", this, data);
					GetHost()?.SendEvent(hitEvent);
				}

				launchTasks.Add(LaunchOneAsync());

				if (i < attackCount - 1 && intervalSeconds > 0f)
					await UniTask.Delay(TimeSpan.FromSeconds(intervalSeconds));
			}

			// 等待所有弹射物命中后再发出完成事件
			await UniTask.WhenAll(launchTasks);

			// 多发射器：全部命中/发射结束后发出执行完成
			var completeEvent = new EntityComponentEvent("ExecutionComplete", this);
			GetHost()?.SendEvent(completeEvent);
		}
	}
}


