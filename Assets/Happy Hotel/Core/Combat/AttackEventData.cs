using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.ValueProcessing;

namespace HappyHotel.Core.Combat
{
    // 攻击事件数据
    public struct AttackEventData
    {
        public BehaviorComponentContainer Attacker;
        public BehaviorComponentContainer Target;
        public int BaseDamage;
        public int FinalDamage;
        public DamageSourceType SourceType;
        public int HitIndex;
        public bool IsLastHitOfAction;
    }
}


