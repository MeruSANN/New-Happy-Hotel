using UnityEngine;

namespace HappyHotel.Intent.Templates
{
	// 多次攻击意图模板：在通用弹射物参数外，提供攻击间隔设置
	[CreateAssetMenu(fileName = "Multi Attack Intent Template", menuName = "Happy Hotel/Intent/Multi Attack Template")]
	public class MultiAttackIntentTemplate : IntentTemplate
	{
		public Sprite projectileSprite;
		[Min(0.1f)] public float animationDuration = 1.0f;
		[Min(0.1f)] public float projectileSpeed = 5.0f;
		[Min(0f)] public float intervalSeconds = 0.2f;
	}
}


