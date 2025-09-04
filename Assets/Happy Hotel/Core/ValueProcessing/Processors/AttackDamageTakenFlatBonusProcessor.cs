using UnityEngine;

namespace HappyHotel.Core.ValueProcessing.Processors
{
    // 受到的“攻击”来源伤害提高的处理器（可叠加）。优先级在护甲之前，使护甲按增幅后数值吸收。
    public class AttackDamageTakenFlatBonusProcessor : IStackableProcessor, IContextualValueProcessor
    {
        private int total;

        public int Priority => 15;
        public ValueChangeType SupportedChangeTypes => ValueChangeType.Decrease;

        public void AddStack(int amount, object provider)
        {
            total += Mathf.Max(0, amount);
        }

        public bool RemoveStack(object provider)
        {
            // 简化实现：不跟踪provider，清零并返回true表示移除成功
            var had = total > 0;
            total = 0;
            return had;
        }

        public int GetStackCount()
        {
            return total;
        }

        public bool HasStacks()
        {
            return total > 0;
        }

        public int GetTotalEffectValue()
        {
            return total;
        }

        public bool HasStackFromProvider(object provider)
        {
            // 简化：不区分provider
            return total > 0;
        }

        public int ProcessValue(int originalValue, ValueChangeType changeType)
        {
            // 无上下文版本：不改变
            return originalValue;
        }

        public int ProcessValue(int originalValue, ValueChangeType changeType, ValueChangeContext context)
        {
            if (changeType != ValueChangeType.Decrease || total <= 0)
                return originalValue;

            if (context.SourceType != DamageSourceType.Attack)
                return originalValue;

            var increased = originalValue + total;
            return increased < 0 ? 0 : increased;
        }
    }
}


