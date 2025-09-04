using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.ValueProcessing;
using HappyHotel.Equipment.Templates;
using HappyHotel.Prop.Components;
using UnityEngine;

namespace HappyHotel.Prop
{
    // 铁剑+道具，被触发时给玩家增加攻击力（逻辑同基础版）
    [AutoInitComponent(typeof(AttackPowerBoosterComponent))]
    public class IronSwordPlusProp : EquipmentPropBase
    {
        private AttackEquipmentValue damageValue = new("伤害值");

        public IronSwordPlusProp()
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

        private void SetupAttackPowerBooster()
        {
            var attackPowerBooster = GetBehaviorComponent<AttackPowerBoosterComponent>();
            if (attackPowerBooster != null)
            {
                attackPowerBooster.SetupAttackPowerBonus(damageValue);
                Debug.Log($"[IronSwordPlusProp] 配置 AttackPowerBoosterComponent: 增加攻击力 +{damageValue}");
            }
        }

        protected override string FormatDescriptionInternal(string formattedDescription)
        {
            return formattedDescription.Replace("{damage}", damageValue.ToString());
        }
    }
}