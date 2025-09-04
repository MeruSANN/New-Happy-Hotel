using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.ValueProcessing;
using HappyHotel.Equipment.Templates;
using HappyHotel.Prop.Components;
using UnityEngine;

namespace HappyHotel.Prop
{
    // 格斗拳套道具，被触发时给玩家增加攻击力，并在回合结束通过Buff回滚
    [AutoInitComponent(typeof(AttackPowerBoosterComponent))]
    [AutoInitComponent(typeof(BuffAdderComponent))]
    public class BoxingGlovesProp : EquipmentPropBase
    {
        private AttackEquipmentValue damageValue = new("伤害值"); // 格斗拳套伤害值

        public BoxingGlovesProp()
        {
            damageValue.Initialize(this);
        }

        protected override void Awake()
        {
            base.Awake();
            SetupComponents();
        }

        public void SetDamage(int newDamage)
        {
            damageValue.SetBaseValue(newDamage);
            SetupComponents();
        }

        public int GetDamage()
        {
            return damageValue;
        }

        protected override void OnTemplateSet()
        {
            base.OnTemplateSet();

            if (template is WeaponTemplate weaponTemplate)
            {
                damageValue.SetBaseValue(weaponTemplate.weaponDamage);
                SetupComponents();
            }
        }

        // 设置攻击力加成与回合末回滚Buff
        private void SetupComponents()
        {
            var booster = GetBehaviorComponent<AttackPowerBoosterComponent>();
            if (booster != null)
            {
                booster.SetupAttackPowerBonus(damageValue);
                Debug.Log($"[BoxingGlovesProp] 配置 AttackPowerBoosterComponent: 增加攻击力 +{damageValue}");
            }

            var buffAdder = GetBehaviorComponent<BuffAdderComponent>();
            if (buffAdder != null)
            {
                // 配置在触发时添加回合末回滚Buff，层数恒为1，面额取当前伤害
                buffAdder.SetBuffType("TurnEndRevertFlatAttackBonus");
                buffAdder.SetBuffSetting(new HappyHotel.Buff.Settings.TurnEndRevertFlatAttackBonusSetting(damageValue.GetFinalValue()));
                Debug.Log($"[BoxingGlovesProp] 配置 BuffAdderComponent: TurnEndRevertFlatAttackBonus perLayer={damageValue.GetFinalValue()} stacks=1");
            }
        }

        protected override string FormatDescriptionInternal(string formattedDescription)
        {
            // 为格斗拳套道具添加特定的占位符替换
            return formattedDescription
                .Replace("{damage}", damageValue.ToString());
        }
    }
}