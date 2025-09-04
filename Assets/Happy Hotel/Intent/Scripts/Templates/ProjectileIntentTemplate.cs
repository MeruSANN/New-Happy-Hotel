using UnityEngine;

namespace HappyHotel.Intent.Templates
{
	// 通用发射物模板：供任意带弹射演出的Intent使用
	[CreateAssetMenu(fileName = "Projectile Intent Template", menuName = "Happy Hotel/Intent/Projectile Template")]
	public class ProjectileIntentTemplate : IntentTemplate
	{
		public Sprite projectileSprite;
		[Min(0.1f)] public float animationDuration = 1.0f;
		[Min(0.1f)] public float projectileSpeed = 5.0f;
	}
}


