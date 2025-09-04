namespace HappyHotel.Core.ValueProcessing.Processors
{
    // 通用数值处理器接口
    public interface IValueProcessor
    {
        // 处理优先级，数值越小优先级越高
        int Priority { get; }

        // 支持的数值变化类型
        ValueChangeType SupportedChangeTypes { get; }

        // 处理数值变化，返回处理后的数值
        int ProcessValue(int originalValue, ValueChangeType changeType);
    }
}