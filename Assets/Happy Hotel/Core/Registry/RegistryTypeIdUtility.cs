using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace HappyHotel.Core.Registry
{
	// 通用工具：根据 Registry 或 RegistrationAttribute 获取所有已注册的 TypeId 列表
	public static class RegistryTypeIdUtility
	{
		private static readonly Dictionary<Type, List<string>> cacheByAttributeType = new();
		private static readonly Dictionary<Type, List<string>> cacheByRegistryType = new();

		public static IEnumerable<string> GetRegisteredTypeIdsByRegistrationAttribute<TRegistrationAttribute>()
			where TRegistrationAttribute : RegistrationAttribute
		{
			var key = typeof(TRegistrationAttribute);
			if (cacheByAttributeType.TryGetValue(key, out var cached)) return cached;

			var list = ScanByAttributeType(typeof(TRegistrationAttribute)).ToList();
			cacheByAttributeType[key] = list;
			return list;
		}

		public static IEnumerable<string> GetRegisteredTypeIdsByRegistry<TRegistry>()
		{
			var key = typeof(TRegistry);
			if (cacheByRegistryType.TryGetValue(key, out var cached)) return cached;

			Type registrationAttributeType = null;
			try
			{
				var instanceProp = key.GetProperty("Instance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				var instance = instanceProp?.GetValue(null);
				if (instance == null)
				{
					Debug.LogWarning($"RegistryTypeIdUtility: {key.Name} 没有可用的单例实例");
					cacheByRegistryType[key] = new List<string>();
					return cacheByRegistryType[key];
				}

				var method = key.GetMethod("GetRegistrationAttributeType", BindingFlags.Instance | BindingFlags.NonPublic);
				registrationAttributeType = method?.Invoke(instance, null) as Type;
			}
			catch (Exception e)
			{
				Debug.LogWarning($"RegistryTypeIdUtility: 反射 {key.Name} 失败: {e.Message}");
			}

			if (registrationAttributeType == null)
			{
				cacheByRegistryType[key] = new List<string>();
				return cacheByRegistryType[key];
			}

			var list = ScanByAttributeType(registrationAttributeType).ToList();
			cacheByRegistryType[key] = list;
			return list;
		}

		public static void ClearCache()
		{
			cacheByAttributeType.Clear();
			cacheByRegistryType.Clear();
		}

		private static IEnumerable<string> ScanByAttributeType(Type registrationAttributeType)
		{
			var result = new List<string>();
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				Type[] types;
				try
				{
					types = assembly.GetTypes();
				}
				catch (ReflectionTypeLoadException ex)
				{
					types = ex.Types.Where(t => t != null).ToArray();
				}
				catch
				{
					continue;
				}

				foreach (var type in types)
				{
					if (type == null) continue;
					try
					{
						var attr = type.GetCustomAttribute(registrationAttributeType) as RegistrationAttribute;
						if (attr != null && !string.IsNullOrEmpty(attr.TypeId)) result.Add(attr.TypeId);
					}
					catch
					{
						// ignored
					}
				}
			}

			return result.Distinct().OrderBy(x => x);
		}
	}
}


