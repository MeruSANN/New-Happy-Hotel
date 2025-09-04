using HappyHotel.Core.BehaviorComponent;

namespace HappyHotel.Core.ValueProcessing
{
    // 伤害来源类型
    public enum DamageSourceType
    {
        Unknown = 0,
        Attack = 1,
        Intent = 2,
        Trap = 3,
        Dot = 4,
        Environment = 5,
        Self = 6
    }

    // 数值变化上下文
    public struct ValueChangeContext
    {
        public DamageSourceType SourceType;
        public IComponentContainer Attacker;

        public static ValueChangeContext None => new ValueChangeContext { SourceType = DamageSourceType.Unknown, Attacker = null };

        public static ValueChangeContext Create(DamageSourceType sourceType, IComponentContainer attacker)
        {
            return new ValueChangeContext { SourceType = sourceType, Attacker = attacker };
        }
    }
}


