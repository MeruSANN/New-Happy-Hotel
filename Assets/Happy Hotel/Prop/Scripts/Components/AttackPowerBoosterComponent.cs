using System;
using System.Collections.Generic;
using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.ValueProcessing.Components;
using HappyHotel.Core.ValueProcessing;
using HappyHotel.Core.ValueProcessing.Modifiers;
using UnityEngine;

namespace HappyHotel.Prop.Components
{
    // 攻击力增加组件，当道具被触发时给触发者增加攻击力
    public class AttackPowerBoosterComponent : BehaviorComponentBase, IEventListener
    {
        private ProcessableValue sourceValue; // 动态来源数值

        // 实现IEventListener接口，监听Trigger事件
        public void OnEvent(BehaviorComponentEvent evt)
        {
            if (evt.EventName == "Trigger" && evt.Data is BehaviorComponentContainer triggerer)
                AddAttackPowerToTriggerer(triggerer);
        }

        // 设置攻击力加成来源
        public void SetupAttackPowerBonus(ProcessableValue source)
        {
            sourceValue = source;
        }
        public int GetAttackPowerBonus()
        {
            return sourceValue.GetFinalValue();
        }

        // 向触发者增加攻击力
        private void AddAttackPowerToTriggerer(BehaviorComponentContainer triggerer)
        {
            var amount = sourceValue?.GetFinalValue() ?? 0;
            if (triggerer == null || amount <= 0)
                return;

            try
            {
                var attackPowerComponent = triggerer.GetBehaviorComponent<AttackPowerComponent>();
                if (attackPowerComponent != null)
                {
                    // 使用无源注册方式给攻击力添加 FlatBonusModifier
                    attackPowerComponent.RegisterAttackModifierWithoutSource<FlatBonusModifier>(amount);
                    Debug.Log($"[AttackPowerBoosterComponent] 给 {triggerer.gameObject.name} 增加攻击力: +{amount}");
                }
                else
                {
                    Debug.LogWarning($"{triggerer.gameObject.name} 没有 AttackPowerComponent");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"增加攻击力失败: {ex.Message}");
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            // 不再清除增益，让增益持续存在
        }
    }
}
