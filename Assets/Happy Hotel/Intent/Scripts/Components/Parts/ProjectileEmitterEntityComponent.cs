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
	// 发射器：负责一次弹射物播放并在命中时派发ApplyOnTarget事件
	public class ProjectileEmitterEntityComponent : EntityComponentBase, IEventListener, ICompletesOnApplyOnTarget
	{
		private Sprite cachedProjectileSprite;
		private float cachedAnimationDuration = 1f;
		private float cachedProjectileSpeed = 5f;
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

		public void OnEvent(EntityComponentEvent evt)
		{
			if (evt.EventName != "Execute") return;
			var senderType = evt.Sender != null ? evt.Sender.GetType().Name : "null";
			EmitOnceAsync().Forget();
		}

		private async UniTask EmitOnceAsync()
		{
			if (intentHost == null || intentHost.Owner == null)
			{
				return;
			}

			var mainCharacter = GameObject.FindGameObjectWithTag("MainCharacter");
			if (mainCharacter == null) { return; }

			var target = mainCharacter.GetComponent<BehaviorComponentContainer>();
			if (target == null) { return; }

			var targetHp = target.GetBehaviorComponent<HitPointValueComponent>();

			var start = intentHost.Owner.transform.position;
			var end = mainCharacter.transform.position;
			if (cachedProjectileSprite != null)
				await ProjectileUtility.Play(cachedProjectileSprite, start, end, cachedProjectileSpeed, cachedAnimationDuration);

			var data = new ApplyOnTargetEventData
			{
				Target = target,
				TargetHp = targetHp,
				HitIndex = 0,
				IsLastHitOfAction = true
			};
			var hitEvent = new EntityComponentEvent("ApplyOnTarget", this, data);
			GetHost()?.SendEvent(hitEvent);

			// 单发射器：命中后立即发出执行完成
			var completeEvent = new EntityComponentEvent("ExecutionComplete", this);
			GetHost()?.SendEvent(completeEvent);
		}
	}
}


