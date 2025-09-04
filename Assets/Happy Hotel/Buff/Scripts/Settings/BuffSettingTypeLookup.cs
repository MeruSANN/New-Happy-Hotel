using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HappyHotel.Buff.Settings
{
	// 标注：声明某Setting用于哪个Buff类型
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class BuffSettingForAttribute : Attribute
	{
		public readonly string typeId;
		public BuffSettingForAttribute(string typeId)
		{
			this.typeId = typeId;
		}
	}

	// 查找：根据Buff类型Id找到对应Setting类型
	public static class BuffSettingTypeLookup
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
					if (!typeof(IBuffSetting).IsAssignableFrom(t)) continue;
					var attr = t.GetCustomAttribute<BuffSettingForAttribute>();
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


