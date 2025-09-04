using System.Collections.Generic;

namespace HappyHotel.Core.ValueProcessing.Modifiers
{
	// 平铺加成：所有来源的加值求和
	public class FlatBonusModifier : IStackableStatModifier
	{
		private readonly Dictionary<object, int> stacks = new();

		public int Priority => 100;

		public int Apply(int currentValue)
		{
			return currentValue + GetTotalEffectValue();
		}

		public void AddStack(int amount, object provider)
		{
			if (amount == 0) return;
			var key = provider ?? this;
			stacks.TryGetValue(key, out var v);
			stacks[key] = v + amount;
		}

		public bool RemoveStack(object provider)
		{
			var key = provider ?? this;
			return stacks.Remove(key);
		}

		public int GetStackCount()
		{
			return stacks.Count;
		}

		public bool HasStacks()
		{
			return stacks.Count > 0;
		}

		public int GetTotalEffectValue()
		{
			var total = 0;
			foreach (var kv in stacks) total += kv.Value;
			return total;
		}

		public bool HasStackFromProvider(object provider)
		{
			var key = provider ?? this;
			return stacks.ContainsKey(key);
		}
	}
}

