using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.ValueProcessing;
using HappyHotel.Equipment.Templates;
using HappyHotel.GameManager;
using UnityEngine;

namespace HappyHotel.Prop
{
	// 触发后在当关内提升费用上限
	public class CostCapCharmProp : EquipmentPropBase
	{
		private readonly EquipmentValue maxCostBonusValue = new("费用上限加成");

		public CostCapCharmProp()
		{
			maxCostBonusValue.Initialize(this);
		}

		protected override void OnTemplateSet()
		{
			base.OnTemplateSet();
			if (template is CostCapCharmTemplate t) maxCostBonusValue.SetBaseValue(Mathf.Max(0, t.maxCostBonus));
		}

		public override void OnTriggerInternal(BehaviorComponentContainer triggerer)
		{
			base.OnTriggerInternal(triggerer);
			var cm = CostManager.Instance;
			if (cm != null) cm.AddLevelMaxCostBonus(maxCostBonusValue.GetFinalValue(), this);
		}

		protected override string FormatDescriptionInternal(string formattedDescription)
		{
			return formattedDescription.Replace("{maxCostBonus}", maxCostBonusValue.GetFinalValue().ToString());
		}
	}
}


