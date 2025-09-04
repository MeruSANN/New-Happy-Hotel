using System.Collections.Generic;
using System.Linq;

namespace HappyHotel.Core.Tag
{
    // Tag系统的工具类
    public static class TagUtils
    {
        // 从多个可标记对象中查找包含指定标签的对象
        public static IEnumerable<T> FindByTag<T>(IEnumerable<T> objects, string tag) where T : ITaggable
        {
            return objects.Where(obj => obj.HasTag(tag));
        }

        // 从多个可标记对象中查找包含任意指定标签的对象
        public static IEnumerable<T> FindByAnyTag<T>(IEnumerable<T> objects, params string[] tags) where T : ITaggable
        {
            return objects.Where(obj => obj.HasAnyTag(tags));
        }

        // 从多个可标记对象中查找包含所有指定标签的对象
        public static IEnumerable<T> FindByAllTags<T>(IEnumerable<T> objects, params string[] tags) where T : ITaggable
        {
            return objects.Where(obj => obj.HasAllTags(tags));
        }

        // 检查两个可标记对象是否有共同标签
        public static bool HasCommonTags(ITaggable obj1, ITaggable obj2)
        {
            var tags1 = obj1.GetTags();
            var tags2 = obj2.GetTags();
            return tags1.Any(tag => tags2.Contains(tag));
        }

        // 获取两个可标记对象的共同标签
        public static IEnumerable<string> GetCommonTags(ITaggable obj1, ITaggable obj2)
        {
            var tags1 = obj1.GetTags();
            var tags2 = obj2.GetTags();
            return tags1.Intersect(tags2);
        }

        // 批量添加标签
        public static void AddTags(ITaggable obj, params string[] tags)
        {
            foreach (var tag in tags) obj.AddTag(tag);
        }

        // 批量移除标签
        public static void RemoveTags(ITaggable obj, params string[] tags)
        {
            foreach (var tag in tags) obj.RemoveTag(tag);
        }
    }
}