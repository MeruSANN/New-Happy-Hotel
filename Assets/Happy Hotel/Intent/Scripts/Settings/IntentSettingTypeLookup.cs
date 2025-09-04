using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.Serialization;

namespace HappyHotel.Intent.Settings
{
    // 标注：声明某Setting用于哪个意图类型
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class IntentSettingForAttribute : Attribute
    {
        public readonly string typeId;
        public IntentSettingForAttribute(string typeId)
        {
            this.typeId = typeId;
        }
    }

    // 查找：根据意图类型Id找到对应Setting类型
    public static class IntentSettingTypeLookup
    {
        private static readonly Dictionary<string, Type> cache = new();
        private static bool initialized = false;

        private static void EnsureInitialized()
        {
            if (initialized) return;
            BuildCache();
            initialized = true;
        }

        private static void BuildCache()
        {
            cache.Clear();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var asm in assemblies)
            {
                Type[] types;
                try { types = asm.GetTypes(); }
                catch (ReflectionTypeLoadException e) { types = e.Types.Where(t => t != null).ToArray(); }
                catch { continue; }

                foreach (var t in types)
                {
                    if (t == null) continue;
                    if (!typeof(IIntentSetting).IsAssignableFrom(t)) continue;
                    var attr = t.GetCustomAttribute<IntentSettingForAttribute>();
                    if (attr == null) continue;
                    if (string.IsNullOrEmpty(attr.typeId)) continue;
                    cache[attr.typeId] = t;
                }
            }
        }

        public static Type GetSettingTypeFor(string typeId)
        {
            if (string.IsNullOrEmpty(typeId)) return null;
            EnsureInitialized();
            cache.TryGetValue(typeId, out var t);
            return t;
        }

        
    }
}


