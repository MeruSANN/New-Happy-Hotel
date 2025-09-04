using HappyHotel.Buff;
using HappyHotel.Buff.Settings;
using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Equipment.Templates;
using HappyHotel.Prop.Components;
using UnityEngine;

namespace HappyHotel.Prop
{
    // 触发增幅护符的Prop：被触发时给触发者添加 NextPropAttackBoostBuff，增幅来自模板
    [AutoInitComponent(typeof(BuffAdderComponent))]
    public class PropBoostCharmProp : EquipmentPropBase
    {
        protected override void OnTemplateSet()
        {
            base.OnTemplateSet();

            var buffAdder = GetBehaviorComponent<BuffAdderComponent>();
            if (buffAdder != null)
            {
                buffAdder.SetBuffType("NextPropAttackBoost");

                var bonus = 0;
                if (template is PropBoostCharmTemplate t) bonus = Mathf.Max(0, t.buffBonus);
                var setting = new NextPropAttackBoostSetting(bonus);
                buffAdder.SetBuffSetting(setting);
            }
        }

        protected override string FormatDescriptionInternal(string formattedDescription)
        {
            var bonus = 0;
            if (template is PropBoostCharmTemplate t) bonus = Mathf.Max(0, t.buffBonus);
            return formattedDescription.Replace("{bonus}", bonus.ToString());
        }
    }
}


