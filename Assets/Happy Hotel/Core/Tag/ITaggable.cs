using System.Collections.Generic;

namespace HappyHotel.Core.Tag
{
    // 可标记对象的接口
    public interface ITaggable
    {
        // 添加标签
        void AddTag(string tag);

        // 移除标签
        void RemoveTag(string tag);

        // 检查是否包含标签
        bool HasTag(string tag);

        // 检查是否包含任意一个标签
        bool HasAnyTag(IEnumerable<string> targetTags);

        // 检查是否包含所有标签
        bool HasAllTags(IEnumerable<string> targetTags);

        // 获取所有标签
        IReadOnlyCollection<string> GetTags();
    }
}