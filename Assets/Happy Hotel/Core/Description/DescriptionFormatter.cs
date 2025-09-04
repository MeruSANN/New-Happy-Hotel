using System.Collections.Generic;

namespace HappyHotel.Core.Description
{
    // 描述格式化工具类，提供统一的描述格式化功能
    public static class DescriptionFormatter
    {
        // 获取对象的格式化描述
        public static string GetFormattedDescription(object obj)
        {
            if (obj is IFormattableDescription formattable) return formattable.GetFormattedDescription();

            return "";
        }

        // 获取对象的描述模板
        public static string GetDescriptionTemplate(object obj)
        {
            if (obj is IFormattableDescription formattable) return formattable.GetDescriptionTemplate();

            return "";
        }

        // 批量获取多个对象的格式化描述
        public static List<string> GetFormattedDescriptions(IEnumerable<object> objects)
        {
            var descriptions = new List<string>();

            foreach (var obj in objects) descriptions.Add(GetFormattedDescription(obj));

            return descriptions;
        }

        // 检查对象是否支持格式化描述
        public static bool SupportsFormattedDescription(object obj)
        {
            return obj is IFormattableDescription;
        }

        // 为UI提供的便捷方法：获取带有默认值的格式化描述
        public static string GetFormattedDescriptionOrDefault(object obj, string defaultDescription = "无描述")
        {
            var description = GetFormattedDescription(obj);
            return string.IsNullOrEmpty(description) ? defaultDescription : description;
        }
    }
}