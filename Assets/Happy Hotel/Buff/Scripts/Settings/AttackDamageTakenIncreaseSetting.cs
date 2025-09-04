using Sirenix.Serialization;

namespace HappyHotel.Buff.Settings
{
    // 受击增伤设置，支持序列化与映射
    [System.Serializable]
    [BuffSettingFor("AttackDamageTakenIncrease")]
    public class AttackDamageTakenIncreaseSetting : IBuffSetting
    {
        [OdinSerialize]
        private int stackCount = 1;

        public AttackDamageTakenIncreaseSetting()
        {
        }

        public AttackDamageTakenIncreaseSetting(int stackCount)
        {
            this.stackCount = stackCount;
        }

        public void ConfigureBuff(BuffBase buff)
        {
            if (buff is AttackDamageTakenIncreaseBuff b)
            {
                b.SetStackCount(stackCount);
            }
        }

        public int GetStackCount()
        {
            return stackCount;
        }
    }
}


