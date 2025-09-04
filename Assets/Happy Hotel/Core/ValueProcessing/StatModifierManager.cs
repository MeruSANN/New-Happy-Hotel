using System;
using System.Collections.Generic;
using System.Linq;
using HappyHotel.Core.ValueProcessing.Modifiers;
using UnityEngine;

namespace HappyHotel.Core.ValueProcessing
{
	// 数值修饰器管理器
	public class StatModifierManager
	{
		private readonly Dictionary<Type, IStackableStatModifier> stackableModifiers = new();
		private List<IStatModifier> regularModifiers = new();
		private bool isDirty;
		public event System.Action onModifiersChanged;

		private void MarkDirtyAndNotify()
		{
			isDirty = true;
			onModifiersChanged?.Invoke();
		}

		public void RegisterModifier(IStatModifier modifier)
		{
			if (modifier is IStackableStatModifier)
			{
				Debug.LogWarning($"尝试将可叠加修饰器 {modifier.GetType().Name} 注册为常规修饰器，请使用 RegisterStackableModifier");
				return;
			}

			if (!regularModifiers.Contains(modifier))
			{
				regularModifiers.Add(modifier);
				if (modifier is Modifiers.IModifierManagerAware aware) aware.BindManager(this);
				MarkDirtyAndNotify();
				Debug.Log($"注册常规修饰器: {modifier.GetType().Name}, 优先级: {modifier.Priority}");
			}
		}

		public void UnregisterModifier(IStatModifier modifier)
		{
			if (regularModifiers.Remove(modifier))
			{
				if (modifier is Modifiers.IModifierManagerAware aware) aware.UnbindManager(this);
				MarkDirtyAndNotify();
				Debug.Log($"注销常规修饰器: {modifier.GetType().Name}");
			}
		}

		// 注册可叠加修饰器（provider: 效果提供者）
		public void RegisterStackableModifier<T>(int amount, object provider) where T : IStackableStatModifier, new()
		{
			var type = typeof(T);
			if (!stackableModifiers.ContainsKey(type))
			{
				stackableModifiers[type] = new T();
				if (stackableModifiers[type] is Modifiers.IModifierManagerAware aware) aware.BindManager(this);
				MarkDirtyAndNotify();
				Debug.Log($"创建新的可叠加修饰器: {type.Name}");
			}

			stackableModifiers[type].AddStack(amount, provider);
			onModifiersChanged?.Invoke();
		}

		public void RegisterStackableModifierWithoutSource<T>(int amount) where T : IStackableStatModifier, new()
		{
			var type = typeof(T);
			if (!stackableModifiers.ContainsKey(type))
			{
				stackableModifiers[type] = new T();
				if (stackableModifiers[type] is Modifiers.IModifierManagerAware aware) aware.BindManager(this);
				MarkDirtyAndNotify();
				Debug.Log($"创建新的无来源可叠加修饰器: {type.Name}");
			}

			stackableModifiers[type].AddStack(amount, null);
			onModifiersChanged?.Invoke();
		}

		// 注销可叠加修饰器的特定提供者
		public void UnregisterStackableModifier<T>(object provider) where T : IStackableStatModifier
		{
			var type = typeof(T);
			if (stackableModifiers.TryGetValue(type, out var modifier))
			{
				modifier.RemoveStack(provider);
				if (!modifier.HasStacks())
				{
					stackableModifiers.Remove(type);
					if (modifier is Modifiers.IModifierManagerAware aware) aware.UnbindManager(this);
					Debug.Log($"移除空的可叠加修饰器: {type.Name}");
				}
				MarkDirtyAndNotify();
				onModifiersChanged?.Invoke();
			}
		}

		public void UnregisterAllStacksOf<T>() where T : IStackableStatModifier
		{
			var type = typeof(T);
			if (stackableModifiers.TryGetValue(type, out var modifier))
			{
				stackableModifiers.Remove(type);
				if (modifier is Modifiers.IModifierManagerAware aware) aware.UnbindManager(this);
				MarkDirtyAndNotify();
				onModifiersChanged?.Invoke();
			}
		}

		public bool HasStackableModifier<T>(object provider) where T : IStackableStatModifier
		{
			var type = typeof(T);
			return stackableModifiers.TryGetValue(type, out var modifier) && modifier.HasStackFromProvider(provider);
		}

		public int ApplyModifiers(int baseValue)
		{
			if (isDirty) SortModifiers();
			var current = baseValue;
			var all = regularModifiers
				.Concat(stackableModifiers.Values)
				.OrderBy(m => m.Priority)
				.ToList();
			foreach (var m in all)
			{
				var nv = m.Apply(current);
				current = nv;
			}
			return current;
		}

		private void SortModifiers()
		{
			regularModifiers = regularModifiers.OrderBy(m => m.Priority).ToList();
			isDirty = false;
		}

		public List<(string Name, int Priority, string Details)> GetModifierInfo()
		{
			if (isDirty) SortModifiers();
			var list = new List<(string, int, string)>();
			foreach (var m in regularModifiers)
				list.Add((m.GetType().Name, m.Priority, "-"));
			foreach (var m in stackableModifiers.Values)
				list.Add((m.GetType().Name, m.Priority, $"Total={m.GetTotalEffectValue()} Stacks={m.GetStackCount()}"));
			return list;
		}

		public void Clear()
		{
			regularModifiers.Clear();
			stackableModifiers.Clear();
			MarkDirtyAndNotify();
		}
	}
}