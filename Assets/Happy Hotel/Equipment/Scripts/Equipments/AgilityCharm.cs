using HappyHotel.Equipment.Templates;
using UnityEngine;

namespace HappyHotel.Equipment
{
	// 增加敏捷上限的装备
	public class AgilityCharm : EquipmentBase
	{
		private int agilityIncrease;

		public int AgilityIncrease => agilityIncrease;

		protected override void OnTemplateSet()
		{
			base.OnTemplateSet();
			if (Template is AgilityCharmTemplate charmTemplate)
			{
				agilityIncrease = Mathf.Max(0, charmTemplate.agilityIncrease);
			}
		}

		protected override string FormatDescriptionInternal(string formattedDescription)
		{
			return formattedDescription.Replace("{agility}", agilityIncrease.ToString());
		}
	}
}


