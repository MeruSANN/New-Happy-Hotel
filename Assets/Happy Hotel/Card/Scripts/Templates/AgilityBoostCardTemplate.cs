using HappyHotel.Equipment.Templates;
using UnityEngine;

namespace HappyHotel.Card.Templates
{
	// 增加敏捷上限的卡牌模板
	[CreateAssetMenu(fileName = "New Agility Boost Card Template", menuName = "Happy Hotel/Item/Agility Boost Card Template")]
	public class AgilityBoostCardTemplate : CardTemplate
	{
		[Header("敏捷设置")] [Tooltip("使用后增加的敏捷上限数值")]
		public int agilityIncrease = 1;
	}
}


