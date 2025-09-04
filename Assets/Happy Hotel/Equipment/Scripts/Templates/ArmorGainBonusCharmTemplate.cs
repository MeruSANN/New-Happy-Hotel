using UnityEngine;

namespace HappyHotel.Equipment.Templates
{
	// 护甲获得增益护符模板：设置给予的Buff层数
	[CreateAssetMenu(fileName = "New Armor Gain Bonus Charm Template", menuName = "Happy Hotel/Item/Armor Gain Bonus Charm Template")]
	public class ArmorGainBonusCharmTemplate : EquipmentTemplate
	{
		[Header("护甲获得额外加成")]
		[Tooltip("给予的护甲额外获得Buff的层数（x点）")]
		public int bonusStacks = 1;
	}
}


