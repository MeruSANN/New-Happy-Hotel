using HappyHotel.Core;
using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.ValueProcessing.Components;
using HappyHotel.Core.ValueProcessing.Processors;
using UnityEngine;

namespace HappyHotel.Buff
{
    // 受到的“攻击”来源伤害提升的Buff：每层+1（可配置），仅对攻击来源生效
    public class AttackDamageTakenIncreaseBuff : BuffBase
    {
        private int stackCount = 1;

        public void SetStackCount(int count)
        {
            stackCount = Mathf.Max(1, count);
            SyncProcessorStacks();
        }

        public void AddStacks(int count)
        {
            if (count <= 0) return;
            stackCount += count;
            SyncProcessorStacks();
        }

        public override void OnApply(IComponentContainer target)
        {
            var container = buffContainer?.GetHost() as BehaviorComponentContainer;
            if (container == null) return;

            SyncProcessorStacks();
        }

        public override void OnRemove(IComponentContainer target)
        {
            var container = buffContainer?.GetHost() as BehaviorComponentContainer;
            if (container == null) return;

            var hp = container.GetBehaviorComponent<HitPointValueComponent>();
            if (hp == null) return;

            // 仅卸载当前Buff提供者的堆叠
            hp.HitPointValue.UnregisterStackableProcessor<AttackDamageTakenFlatBonusProcessor>(this);
            buffContainer?.NotifyBuffsChanged();
        }

        public override int GetValue()
        {
            return stackCount;
        }

        public override void OnTurnStart(int turnNumber)
        {
            var container = buffContainer?.GetHost() as BehaviorComponentContainer;
            if (container == null)
            {
                RequestRemoveSelf();
                return;
            }

            stackCount = Mathf.Max(0, stackCount - 1);
            SyncProcessorStacks();

            if (stackCount <= 0)
            {
                RequestRemoveSelf();
            }
        }

        public override BuffMergeResult TryMergeWith(BuffBase newBuff)
        {
            if (newBuff is AttackDamageTakenIncreaseBuff other)
            {
                stackCount += other.stackCount;
                SyncProcessorStacks();
                return BuffMergeResult.CreateMerge(this);
            }
            return BuffMergeResult.CreateCoexist();
        }

        protected override string FormatDescriptionInternal(string formattedDescription)
        {
            return formattedDescription.Replace("{layer}", stackCount.ToString());
        }

        private void SyncProcessorStacks()
        {
            var container = buffContainer?.GetHost() as BehaviorComponentContainer;
            if (container == null) return;
            var hp = container.GetBehaviorComponent<HitPointValueComponent>();
            if (hp == null) return;

            hp.HitPointValue.UnregisterStackableProcessor<AttackDamageTakenFlatBonusProcessor>(this);
            if (stackCount > 0)
            {
                hp.HitPointValue.RegisterStackableProcessor<AttackDamageTakenFlatBonusProcessor>(stackCount, this);
            }
            buffContainer?.NotifyBuffsChanged();
        }
    }
}


