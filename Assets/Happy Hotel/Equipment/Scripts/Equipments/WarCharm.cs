using HappyHotel.Core.ValueProcessing;
using HappyHotel.Equipment.Templates;
using UnityEngine;

namespace HappyHotel.Equipment
{
	// 战之护符装备：增加攻击力并获得护甲
	public class WarCharm : EquipmentBase
	{
		private readonly AttackEquipmentValue attackDamageValue = new("攻击伤害");
		private readonly EquipmentValue armorAmountValue = new("护甲值");

		public WarCharm()
		{
			attackDamageValue.Initialize(this);
			armorAmountValue.Initialize(this);
		}

		public int AttackDamage => attackDamageValue?.GetFinalValue() ?? 0;
		public int ArmorAmount => armorAmountValue?.GetFinalValue() ?? 0;

		protected override void OnTemplateSet()
		{
			base.OnTemplateSet();
			if (Template is WarCharmTemplate t)
			{
				attackDamageValue.SetBaseValue(Mathf.Max(0, t.weaponDamage));
				armorAmountValue.SetBaseValue(Mathf.Max(0, t.armorAmount));
			}
		}

		protected override string FormatDescriptionInternal(string formattedDescription)
		{
			return formattedDescription
				.Replace("{damage}", AttackDamage.ToString())
				.Replace("{armor}", ArmorAmount.ToString());
		}
	}
}


