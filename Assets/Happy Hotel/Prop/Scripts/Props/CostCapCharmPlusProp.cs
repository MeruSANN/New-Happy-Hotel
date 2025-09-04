using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.ValueProcessing;
using HappyHotel.Equipment.Templates;
using HappyHotel.GameManager;
using UnityEngine;

namespace HappyHotel.Prop
{
	// 提升费用上限+ 道具
	public class CostCapCharmPlusProp : EquipmentPropBase
	{
		private readonly EquipmentValue maxCostBonusValue = new("费用上限加成");

		public CostCapCharmPlusProp()
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