using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.ValueProcessing;
using HappyHotel.Core.ValueProcessing.Modifiers;
using HappyHotel.Equipment.Templates;
using HappyHotel.Prop.Components;
using UnityEngine;

namespace HappyHotel.Prop
{
    // 铁剑道具，被触发时给玩家增加攻击力
    [AutoInitComponent(typeof(AttackPowerBoosterComponent))]
    public class IronSwordProp : EquipmentPropBase
    {
        private AttackEquipmentValue damageValue = new("伤害值"); // 铁剑默认伤害值

        public IronSwordProp()
        {
            damageValue.Initialize(this);
        }

        protected override void Awake()
        {
            base.Awake();
            SetupAttackPowerBooster();
        }

        public void SetDamage(int newDamage)
        {
            damageValue.SetBaseValue(newDamage);
            SetupAttackPowerBooster();
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
                SetupAttackPowerBooster();
            }
        }

        // 设置攻击力增加组件
        private void SetupAttackPowerBooster()
        {
            var attackPowerBooster = GetBehaviorComponent<AttackPowerBoosterComponent>();
            if (attackPowerBooster != null)
            {
                // 配置攻击力加成
                attackPowerBooster.SetupAttackPowerBonus(damageValue);
                Debug.Log($"[IronSwordProp] 配置 AttackPowerBoosterComponent: 增加攻击力 +{damageValue}");
            }
        }

        protected override string FormatDescriptionInternal(string formattedDescription)
        {
            return formattedDescription
                .Replace("{damage}", damageValue.ToString());
        }
    }
}