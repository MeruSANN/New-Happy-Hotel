namespace HappyHotel.Core.ValueProcessing.Processors
{
    // 护甲数值处理器 - 减少伤害
    public class ArmorValueProcessor : IValueProcessor
    {
        private readonly ArmorValue armorValue;

        public ArmorValueProcessor(ArmorValue armorValue)
        {
            this.armorValue = armorValue;
        }

        public int Priority => 20; // 护甲优先级在格挡之后
        public ValueChangeType SupportedChangeTypes => ValueChangeType.Decrease; // 只处理减少操作

        public int ProcessValue(int originalValue, ValueChangeType changeType)
        {
            if (armorValue == null || originalValue <= 0 || changeType != ValueChangeType.Decrease)
                return originalValue;

            // 使用护甲吸收伤害
            return armorValue.ConsumeArmor(originalValue);
        }
    }
}