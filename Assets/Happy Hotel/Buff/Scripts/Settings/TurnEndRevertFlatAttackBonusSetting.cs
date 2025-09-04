using Sirenix.Serialization;
using UnityEngine;

namespace HappyHotel.Buff.Settings
{
	// 回合结束回退平攻加成设置，支持序列化与映射
	[System.Serializable]
	[BuffSettingFor("TurnEndRevertFlatAttackBonus")]
	public class TurnEndRevertFlatAttackBonusSetting : IBuffSetting
	{
		[OdinSerialize]
		public int stacks = 1;

		public TurnEndRevertFlatAttackBonusSetting()
		{
		}

		public TurnEndRevertFlatAttackBonusSetting(int stacks = 1)
		{
			this.stacks = Mathf.Max(1, stacks);
		}

		public void ConfigureBuff(BuffBase buff)
		{
			var b = buff as TurnEndRevertFlatAttackBonusBuff;
			if (b == null) return;
			b.SetStackCount(stacks);
		}
	}
}


