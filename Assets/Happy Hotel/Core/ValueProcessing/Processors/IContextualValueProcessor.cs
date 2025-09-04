namespace HappyHotel.Core.ValueProcessing.Processors
{
    // 上下文感知处理器接口（可选实现）。
    public interface IContextualValueProcessor : IValueProcessor
    {
        int ProcessValue(int originalValue, ValueChangeType changeType, ValueChangeContext context);
    }
}


