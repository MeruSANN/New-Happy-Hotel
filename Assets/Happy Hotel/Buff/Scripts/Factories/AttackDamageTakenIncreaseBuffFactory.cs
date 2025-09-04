using HappyHotel.Buff.Factories;

namespace HappyHotel.Buff
{
    // 工厂：用于创建 AttackDamageTakenIncreaseBuff
    [BuffRegistration("AttackDamageTakenIncrease",
        "Templates/Attack Damage Taken Increase Buff Template")]
    public class AttackDamageTakenIncreaseBuffFactory : BuffFactoryBase<AttackDamageTakenIncreaseBuff>
    {
    }
}


