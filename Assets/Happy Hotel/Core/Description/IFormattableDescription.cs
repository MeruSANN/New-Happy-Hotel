namespace HappyHotel.Core.Description
{
    // 可格式化描述接口，用于统一处理各种对象的描述格式化
    public interface IFormattableDescription
    {
        // 获取格式化后的描述文本
        string GetFormattedDescription();

        // 获取原始描述模板（包含占位符）
        string GetDescriptionTemplate();
    }
}