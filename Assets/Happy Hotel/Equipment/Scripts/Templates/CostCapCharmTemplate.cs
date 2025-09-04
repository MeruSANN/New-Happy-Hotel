using UnityEngine;

namespace HappyHotel.Equipment.Templates
{
	// 提供当关费用上限加成的装备模板
	[CreateAssetMenu(fileName = "New Cost Cap Charm Template", menuName = "Happy Hotel/Item/Cost Cap Charm Template")]
	public class CostCapCharmTemplate : EquipmentTemplate
	{
		[Header("费用上限加成设置")] [Tooltip("触发后在当前关卡内增加的费用上限数值")]
		public int maxCostBonus = 1;
	}
}


