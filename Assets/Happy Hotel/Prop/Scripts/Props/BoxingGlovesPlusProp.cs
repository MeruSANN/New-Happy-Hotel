using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.ValueProcessing;
using HappyHotel.Equipment.Templates;
using HappyHotel.Prop.Components;
using UnityEngine;

namespace HappyHotel.Prop
{
    // 格斗拳套+道具（与基础版一致）
    [AutoInitComponent(typeof(BuffAdderComponent))]
    public class BoxingGlovesPlusProp : EquipmentPropBase
    {
        private EquipmentValue buffDamageValue = new("攻击伤害");

        public BoxingGlovesPlusProp()
        {
            buffDamageValue.Initialize(this);
        }

        protected override void Awake()
        {
            base.Awake();
            UpdateBuffAdder();
        }

        protected override void OnTemplateSet()
        {
            base.OnTemplateSet();

            if (template is WeaponTemplate t)
            {
                buffDamageValue.SetBaseValue(t.weaponDamage);
                UpdateBuffAdder();
            }
        }

        private void UpdateBuffAdder()
        {
            var buffAdder = GetBehaviorComponent<BuffAdderComponent>();
            if (buffAdder != null)
            {
                buffAdder.SetBuffDamage(buffDamageValue);
            }
        }

        protected override string FormatDescriptionInternal(string formattedDescription)
        {
            return formattedDescription.Replace("{damage}", buffDamageValue.ToString());
        }
    }
}