using System.Collections.Generic;
using HappyHotel.GameManager;

namespace HappyHotel.Core.ValueProcessing.Modifiers
{
	// 临时平铺加成：所有来源的加值求和，回合结束后自动移除
	public class TemporaryFlatBonusModifier : IStackableStatModifier, IModifierManagerAware
	{
		private readonly Dictionary<object, int> stacks = new();
		private StatModifierManager boundManager;

		public int Priority => 100;

		public TemporaryFlatBonusModifier()
		{
			// 监听敌人回合结束事件，自动移除自身
			TurnManager.onEnemyTurnEnd += OnEnemyTurnEnd;
		}

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

		// 敌人回合结束时自动移除自身
		private void OnEnemyTurnEnd(int turnNumber)
		{
			// 通过管理器卸载自身所有堆叠
			boundManager?.UnregisterAllStacksOf<TemporaryFlatBonusModifier>();
			// 取消监听事件
			TurnManager.onEnemyTurnEnd -= OnEnemyTurnEnd;
		}

		public void BindManager(StatModifierManager manager)
		{
			boundManager = manager;
		}

		public void UnbindManager(StatModifierManager manager)
		{
			if (boundManager == manager) boundManager = null;
		}
	}
}
