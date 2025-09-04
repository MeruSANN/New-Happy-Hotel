using System.Collections.Generic;

namespace HappyHotel.Core.ValueProcessing.Processors
{
	// 护甲获得平铺加成处理器：当护甲数值增加时，按叠加层数额外增加同等数值
	public class ArmorGainFlatBonusProcessor : IStackableProcessor, IProcessorManagerAware
	{
		private readonly Dictionary<object, int> stacks = new();
		private ValueProcessorManager boundManager;

		public int Priority => 18; // 护甲增益在常规加成前应用
		public ValueChangeType SupportedChangeTypes => ValueChangeType.Increase;

		public int ProcessValue(int originalValue, ValueChangeType changeType)
		{
			var bonus = GetTotalEffectValue();
			return originalValue + bonus;
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

		public void BindManager(ValueProcessorManager manager)
		{
			boundManager = manager;
		}

		public void UnbindManager(ValueProcessorManager manager)
		{
			if (boundManager == manager) boundManager = null;
		}
	}
}


