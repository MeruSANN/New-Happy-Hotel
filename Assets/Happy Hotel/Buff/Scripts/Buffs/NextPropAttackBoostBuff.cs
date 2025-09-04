using System.Collections.Generic;
using System.Linq;
using HappyHotel.Buff.Components;
using HappyHotel.Core;
using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.ValueProcessing;
using HappyHotel.Core.ValueProcessing.Components;
using HappyHotel.Core.ValueProcessing.Modifiers;
using HappyHotel.Prop;
using HappyHotel.Prop.Components;
using UnityEngine;

namespace HappyHotel.Buff
{
    // 下一次道具触发的攻击力提升Buff：在下一次触发包含AttackEquipmentValue的道具时，为其所有攻击值添加一次性平铺加成
    public class NextPropAttackBoostBuff : BuffBase
    {
        private int bonus = 0;
        private PropTriggerComponent triggerComponent;
        private readonly List<AttackEquipmentValue> appliedValues = new();
        private bool appliedThisTrigger;

        public void SetBonus(int value)
        {
            bonus = Mathf.Max(0, value);
        }

        public override void OnApply(IComponentContainer target)
        {
            if (target is not BehaviorComponentContainer container) return;
            triggerComponent = container.GetBehaviorComponent<PropTriggerComponent>() ?? container.AddBehaviorComponent<PropTriggerComponent>();
            if (triggerComponent != null)
            {
                triggerComponent.onBeforePropTrigger += BeforePropTrigger;
                triggerComponent.onAfterPropTrigger += AfterPropTrigger;
            }
        }

        public override void OnRemove(IComponentContainer target)
        {
            if (triggerComponent != null)
            {
                triggerComponent.onBeforePropTrigger -= BeforePropTrigger;
                triggerComponent.onAfterPropTrigger -= AfterPropTrigger;
            }
            triggerComponent = null;
        }

        private void BeforePropTrigger(PropBase prop)
        {
            if (prop == null || bonus <= 0) return;

            var collector = prop.GetBehaviorComponent<ProcessableValueCollectorBehaviorComponent>();
            var dict = collector?.GetProcessableValues();
            if (dict == null) return;

            var attackValues = dict.Values.OfType<AttackEquipmentValue>().ToList();
            if (attackValues.Count == 0) return;

            appliedValues.Clear();
            foreach (var v in attackValues)
            {
                v.RegisterStackableModifier<HappyHotel.Core.ValueProcessing.Modifiers.FlatBonusModifier>(bonus, this);
                appliedValues.Add(v);
            }
            appliedThisTrigger = true;
        }

        private void AfterPropTrigger(PropBase prop)
        {
            if (!appliedThisTrigger) return;
            foreach (var v in appliedValues) v.UnregisterStackableModifier<HappyHotel.Core.ValueProcessing.Modifiers.FlatBonusModifier>(this);
            appliedValues.Clear();
            appliedThisTrigger = false;
            RequestRemoveSelf();
        }

        protected override string FormatDescriptionInternal(string formattedDescription)
        {
            return formattedDescription.Replace("{bonus}", bonus.ToString());
        }

        public override void OnTurnEnd(int turnNumber)
        {
            RequestRemoveSelf();
        }

        public override int GetValue()
        {
            return bonus;
        }
    }
}


