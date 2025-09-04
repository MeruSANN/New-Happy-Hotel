using HappyHotel.Core;
using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.ValueProcessing.Components;
using UnityEngine;

namespace HappyHotel.Buff
{
	// 回合结束按层数回滚无源平铺攻击加成的Buff
	public class TurnEndRevertFlatAttackBonusBuff : BuffBase
	{
		private int stackCount = 1; // 层数

		public void SetStackCount(int count)
		{
			stackCount = Mathf.Max(1, count);
		}

		public void AddStacks(int count)
		{
			if (count <= 0) return;
			stackCount += count;
		}

		public override void OnApply(IComponentContainer target)
		{
			// 无需立即处理，回合结束时统一回滚
		}

		public override void OnRemove(IComponentContainer target)
		{
			// 无需特殊处理
		}

		public override void OnTurnEnd(int turnNumber)
		{
			var hostContainer = buffContainer?.GetHost() as BehaviorComponentContainer;
			if (hostContainer == null) { RequestRemoveSelf(); return; }

			var attackPower = hostContainer.GetBehaviorComponent<AttackPowerComponent>();
			if (attackPower != null && stackCount > 0)
			{
				// 使用负值注册无源平铺加成，实现回滚
				attackPower.RegisterAttackModifierWithoutSource<HappyHotel.Core.ValueProcessing.Modifiers.FlatBonusModifier>(-stackCount);
			}

			// 回合结束后移除自身
			RequestRemoveSelf();
		}

		public override int GetValue()
		{
			return stackCount;
		}

		public override BuffMergeResult TryMergeWith(BuffBase newBuff)
		{
			if (newBuff is TurnEndRevertFlatAttackBonusBuff other)
			{
				stackCount += other.stackCount;
				return BuffMergeResult.CreateMerge(this);
			}
			return BuffMergeResult.CreateCoexist();
		}

		protected override string FormatDescriptionInternal(string formattedDescription)
		{
			return formattedDescription
				.Replace("{layer}", stackCount.ToString());
		}
	}
}


