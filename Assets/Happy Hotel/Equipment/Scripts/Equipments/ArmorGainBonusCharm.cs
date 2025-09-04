using HappyHotel.Core.ValueProcessing;
using HappyHotel.Equipment.Templates;
using UnityEngine;

namespace HappyHotel.Equipment
{
	// 护甲获得增益护符装备：给予护甲加成Buff的层数
	public class ArmorGainBonusCharm : EquipmentBase
	{
		private readonly EquipmentValue bonusStacksValue = new("护甲额外获得层数");

		public ArmorGainBonusCharm()
		{
			bonusStacksValue.Initialize(this);
		}

		public int BonusStacks => bonusStacksValue?.GetFinalValue() ?? 0;

		protected override void OnTemplateSet()
		{
			base.OnTemplateSet();
			if (Template is ArmorGainBonusCharmTemplate t)
			{
				bonusStacksValue.SetBaseValue(Mathf.Max(1, t.bonusStacks));
			}
		}

		protected override string FormatDescriptionInternal(string formattedDescription)
		{
			return formattedDescription.Replace("{stacks}", BonusStacks.ToString());
		}
	}
}


