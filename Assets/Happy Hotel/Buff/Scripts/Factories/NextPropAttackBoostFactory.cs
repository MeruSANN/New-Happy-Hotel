using HappyHotel.Buff.Factories;

namespace HappyHotel.Buff
{
    // 工厂：用于创建 NextPropAttackBoostBuff
    [BuffRegistration("NextPropAttackBoost", 
        "Templates/Next Prop Attack Boost Buff Template")]
    public class NextPropAttackBoostFactory : BuffFactoryBase<NextPropAttackBoostBuff>
    {
    }
}


