namespace HappyHotel.Core.ValueProcessing.Processors
{
    // 格挡数值处理器 - 减少伤害
    public class BlockValueProcessor : IValueProcessor
    {
        private readonly BlockValue blockValue;

        public BlockValueProcessor(BlockValue blockValue)
        {
            this.blockValue = blockValue;
        }

        public int Priority => 10; // 格挡优先级最高
        public ValueChangeType SupportedChangeTypes => ValueChangeType.Decrease; // 只处理减少操作

        public int ProcessValue(int originalValue, ValueChangeType changeType)
        {
            if (blockValue == null || originalValue <= 0 || changeType != ValueChangeType.Decrease)
                return originalValue;

            // 使用格挡吸收伤害
            return blockValue.ConsumeBlock(originalValue);
        }
    }
}