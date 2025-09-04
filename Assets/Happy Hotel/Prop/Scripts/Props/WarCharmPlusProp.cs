using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.ValueProcessing;
using HappyHotel.Equipment.Templates;
using HappyHotel.Prop.Components;
using UnityEngine;

namespace HappyHotel.Prop
{
	// 战之护符+道具（与基础版一致）
	[AutoInitComponent(typeof(AttackPowerBoosterComponent))]
	[AutoInitComponent(typeof(ArmorAdderComponent))]
	public class WarCharmPlusProp : EquipmentPropBase
	{
		private readonly AttackEquipmentValue attackDamageValue = new("攻击伤害");
		private readonly EquipmentValue armorAmountValue = new("护甲值");

		public WarCharmPlusProp()
		{
			attackDamageValue.Initialize(this);
			armorAmountValue.Initialize(this);
		}

		protected override void Awake()
		{
			base.Awake();
			SetupComponents();
		}

		protected override void OnTemplateSet()
		{
			base.OnTemplateSet();
			if (template is WarCharmTemplate t)
			{
				attackDamageValue.SetBaseValue(Mathf.Max(0, t.weaponDamage));
				armorAmountValue.SetBaseValue(Mathf.Max(0, t.armorAmount));
				SetupComponents();
			}
		}

		private void SetupComponents()
		{
			var atkBooster = GetBehaviorComponent<AttackPowerBoosterComponent>();
			if (atkBooster != null) atkBooster.SetupAttackPowerBonus(attackDamageValue);

			var armorAdder = GetBehaviorComponent<ArmorAdderComponent>();
			if (armorAdder != null) armorAdder.SetArmorAmount(armorAmountValue);
		}

		protected override string FormatDescriptionInternal(string formattedDescription)
		{
			return formattedDescription
				.Replace("{damage}", attackDamageValue.ToString())
				.Replace("{armor}", armorAmountValue.ToString());
		}
	}
}