using HappyHotel.Equipment.Templates;
using UnityEngine;

namespace HappyHotel.Card.Templates
{
	// 攻击装备临时加成卡牌模板
	[CreateAssetMenu(fileName = "New Attack Equip Temp Bonus Card Template", menuName = "Happy Hotel/Item/Attack Equip Temp Bonus Card Template")]
	public class AttackEquipTempBonusCardTemplate : CardTemplate
	{
		[Header("效果设置")] [Tooltip("本回合对目标道具的所有攻击数值的临时加成")]
		public int damageBonus = 2;
	}
}


