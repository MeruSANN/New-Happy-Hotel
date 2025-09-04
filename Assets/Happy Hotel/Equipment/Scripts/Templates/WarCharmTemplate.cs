using UnityEngine;

namespace HappyHotel.Equipment.Templates
{
	// 战之护符模板：同时提供攻击力与护甲数值
	[CreateAssetMenu(fileName = "New War Charm Template", menuName = "Happy Hotel/Item/War Charm Template")]
	public class WarCharmTemplate : EquipmentTemplate
	{
		[Header("攻击设置")] [Tooltip("增加的攻击伤害数值")]
		public int weaponDamage = 3;

		[Header("护甲设置")] [Tooltip("获得的护甲数值")]
		public int armorAmount = 2;
	}
}


