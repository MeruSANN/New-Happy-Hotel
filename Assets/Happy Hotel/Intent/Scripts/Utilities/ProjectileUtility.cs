using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace HappyHotel.Intent.Utilities
{
	// 通用发射物工具
	public static class ProjectileUtility
	{
		public static async UniTask Play(Sprite sprite, Vector3 startPosition, Vector3 endPosition,
			float projectileSpeed, float maxDuration,
			string sortingLayerName = "UI", int sortingOrder = 10)
		{
			if (sprite == null)
				return;

			var projectile = new GameObject("Projectile");
			var spriteRenderer = projectile.AddComponent<SpriteRenderer>();
			spriteRenderer.sprite = sprite;
			spriteRenderer.sortingLayerName = sortingLayerName;
			spriteRenderer.sortingOrder = sortingOrder;

			projectile.transform.position = startPosition;

			var distance = Vector3.Distance(startPosition, endPosition);
			var travelTime = projectileSpeed > 0f ? distance / projectileSpeed : maxDuration;
			travelTime = Mathf.Clamp(travelTime, 0f, Mathf.Max(0.01f, maxDuration));

			var elapsed = 0f;
			while (elapsed < travelTime)
			{
				elapsed += Time.deltaTime;
				var t = Mathf.Clamp01(elapsed / travelTime);
				projectile.transform.position = Vector3.Lerp(startPosition, endPosition, t);
				await UniTask.Yield();
			}

			projectile.transform.position = endPosition;
			if (projectile != null) UnityEngine.Object.Destroy(projectile);
		}
	}
}


