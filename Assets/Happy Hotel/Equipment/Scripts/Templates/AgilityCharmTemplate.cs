using UnityEngine;

namespace HappyHotel.Equipment.Templates
{
	// 敏捷护符模板，配置敏捷上限增加值
	[CreateAssetMenu(fileName = "New Agility Charm Template", menuName = "Happy Hotel/Item/Agility Charm Template")]
	public class AgilityCharmTemplate : EquipmentTemplate
	{
		[Header("敏捷设置")] [Tooltip("触发后增加的敏捷上限数值")]
		public int agilityIncrease = 1;
	}
}


