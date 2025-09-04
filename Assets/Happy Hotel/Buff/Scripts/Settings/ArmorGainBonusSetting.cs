using Sirenix.Serialization;

namespace HappyHotel.Buff.Settings
{
	// 护甲增益Buff设置，支持序列化与构造函数兼容
	[System.Serializable]
	[BuffSettingFor("ArmorGainBonus")]
	public class ArmorGainBonusSetting : IBuffSetting
	{
		[OdinSerialize]
		private int stackCount = 1;

		public ArmorGainBonusSetting()
		{
		}

		public ArmorGainBonusSetting(int stackCount = 1)
		{
			this.stackCount = stackCount;
		}

		public void ConfigureBuff(BuffBase buff)
		{
			if (buff is ArmorGainBonusBuff b) b.SetStackCount(stackCount);
		}

		public int GetStackCount()
		{
			return stackCount;
		}
	}
}


