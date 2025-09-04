using HappyHotel.Core.ValueProcessing;
using HappyHotel.Equipment.Templates;
using UnityEngine;

namespace HappyHotel.Equipment
{
	// 当关提升费用上限的饰品装备
	public class CostCapCharm : EquipmentBase
	{
		private readonly EquipmentValue maxCostBonusValue = new("费用上限加成");

		public CostCapCharm()
		{
			maxCostBonusValue.Initialize(this);
		}

		public int MaxCostBonus => maxCostBonusValue?.GetFinalValue() ?? 0;

		public void SetMaxCostBonus(int value)
		{
			maxCostBonusValue.SetBaseValue(Mathf.Max(0, value));
		}

		protected override void OnTemplateSet()
		{
			base.OnTemplateSet();

			if (Template is CostCapCharmTemplate t)
			{
				maxCostBonusValue.SetBaseValue(Mathf.Max(0, t.maxCostBonus));
			}
		}

		protected override string FormatDescriptionInternal(string formattedDescription)
		{
			return formattedDescription.Replace("{maxCostBonus}", MaxCostBonus.ToString());
		}
	}
}


