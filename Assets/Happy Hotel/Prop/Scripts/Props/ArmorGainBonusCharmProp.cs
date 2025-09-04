using HappyHotel.Buff;
using HappyHotel.Buff.Components;
using HappyHotel.Buff.Settings;
using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Equipment.Templates;
using HappyHotel.Prop.Components;
using UnityEngine;

namespace HappyHotel.Prop
{
	// 护甲获得增益护符道具：触发时给予护甲额外获得Buff若干层
	[AutoInitComponent(typeof(BuffAdderComponent))]
	public class ArmorGainBonusCharmProp : EquipmentPropBase
	{
		private int bonusStacks = 1;

		protected override void OnTemplateSet()
		{
			base.OnTemplateSet();
			if (template is ArmorGainBonusCharmTemplate t)
			{
				bonusStacks = Mathf.Max(1, t.bonusStacks);
				SetupBuffAdder();
			}
		}

		private void SetupBuffAdder()
		{
			var adder = GetBehaviorComponent<BuffAdderComponent>();
			if (adder != null)
			{
				adder.SetBuffType("ArmorGainBonus");
				adder.SetBuffSetting(new ArmorGainBonusSetting(bonusStacks));
			}
		}

		protected override string FormatDescriptionInternal(string formattedDescription)
		{
			return formattedDescription.Replace("{stacks}", bonusStacks.ToString());
		}
	}
}


