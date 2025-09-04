using System;

namespace HappyHotel.Core.ValueProcessing
{
    // 数值变化类型
    [Flags]
    public enum ValueChangeType
    {
        None = 0,
        Increase = 1 << 0, // 数值增加
        Decrease = 1 << 1, // 数值减少
        All = Increase | Decrease // 所有变化
    }
}