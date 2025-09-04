using Sirenix.Serialization;

namespace HappyHotel.Buff.Settings
{
    // 下一次道具攻击提升设置，支持序列化与映射
    [System.Serializable]
    [BuffSettingFor("NextPropAttackBoost")]
    public class NextPropAttackBoostSetting : IBuffSetting
    {
        [OdinSerialize]
        public int bonus = 1;

        public NextPropAttackBoostSetting()
        {
        }

        public NextPropAttackBoostSetting(int bonus)
        {
            this.bonus = bonus;
        }

        public void ConfigureBuff(BuffBase buff)
        {
            if (buff is NextPropAttackBoostBuff b) b.SetBonus(bonus);
        }
    }
}


