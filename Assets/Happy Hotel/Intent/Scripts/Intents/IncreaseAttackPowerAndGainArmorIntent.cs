using HappyHotel.Core.EntityComponent;
using Cysharp.Threading.Tasks;
using HappyHotel.Intent.Components.Parts;

namespace HappyHotel.Intent
{
	// 意图：增加攻击力并获得护甲（组件组合：加攻+自护甲）
	[AutoInitEntityComponent(typeof(IncreaseAttackPowerEntityComponent))]
	[AutoInitEntityComponent(typeof(SelfArmorEntityComponent))]
	public class IncreaseAttackPowerAndGainArmorIntent : IntentBase
	{
		private int attackAmount;
		private int armorAmount;

		public void SetAttackAmount(int value)
		{
			attackAmount = value < 0 ? 0 : value;
			var apComp = GetEntityComponent<IncreaseAttackPowerEntityComponent>();
			apComp?.SetAmount(attackAmount);
		}

		public void SetArmorAmount(int value)
		{
			armorAmount = value < 0 ? 0 : value;
			var armorComp = GetEntityComponent<SelfArmorEntityComponent>();
			armorComp?.SetAmount(armorAmount);
		}

		public override string GetDisplayValue()
		{
			return $"{attackAmount}({armorAmount})";
		}
	}
}


