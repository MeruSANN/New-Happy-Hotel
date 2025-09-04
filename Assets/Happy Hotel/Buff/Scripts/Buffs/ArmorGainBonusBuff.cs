using HappyHotel.Core;
using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.ValueProcessing.Components;
using HappyHotel.Core.ValueProcessing.Processors;
using UnityEngine;

namespace HappyHotel.Buff
{
	// 获得护甲值时额外获得x点（x为层数）的Buff
	public class ArmorGainBonusBuff : BuffBase
	{
		public int StackCount { get; private set; } = 1;

		public void SetStackCount(int count)
		{
			StackCount = Mathf.Max(1, count);
		}

		public override void OnApply(IComponentContainer target)
		{
			if (target is BehaviorComponentContainer behaviorContainer)
			{
				var armorComponent = behaviorContainer.GetBehaviorComponent<ArmorValueComponent>();
				if (armorComponent != null && armorComponent.ArmorValue != null)
				{
					armorComponent.ArmorValue.RegisterStackableProcessor<ArmorGainFlatBonusProcessor>(StackCount, this);
					Debug.Log($"{behaviorContainer.Name} 获得护甲增益Buff，层数: {StackCount}");
				}
			}
		}

		public override void OnRemove(IComponentContainer target)
		{
			if (target is BehaviorComponentContainer behaviorContainer)
			{
				var armorComponent = behaviorContainer.GetBehaviorComponent<ArmorValueComponent>();
				if (armorComponent != null && armorComponent.ArmorValue != null)
				{
					armorComponent.ArmorValue.UnregisterStackableProcessor<ArmorGainFlatBonusProcessor>(this);
					Debug.Log($"{behaviorContainer.Name} 的护甲增益Buff已移除");
				}
			}
		}

		public override int GetValue()
		{
			return StackCount;
		}

		public override BuffMergeResult TryMergeWith(BuffBase newBuff)
		{
			if (newBuff is ArmorGainBonusBuff other)
			{
				StackCount += Mathf.Max(1, other.StackCount);
				return BuffMergeResult.CreateMerge(this);
			}
			return BuffMergeResult.CreateCoexist();
		}

		protected override string FormatDescriptionInternal(string formattedDescription)
		{
			return formattedDescription.Replace("{stack}", StackCount.ToString());
		}
	}
}


