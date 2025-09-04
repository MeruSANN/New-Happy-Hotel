using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.ValueProcessing;
using HappyHotel.Equipment.Templates;
using HappyHotel.Prop.Components;

namespace HappyHotel.Prop
{
    // 轻甲道具，被触发时立刻获得护甲值
    [AutoInitComponent(typeof(ArmorAdderComponent))]
    public class LightArmorProp : EquipmentPropBase
    {
        private EquipmentValue armorAmountValue = new("护甲值"); // 默认护甲值

        public LightArmorProp()
        {
            armorAmountValue.Initialize(this);
        }

        protected override void Awake()
        {
            base.Awake();
            UpdateArmorAdder();
        }

        public void SetArmorAmount(int newArmorAmount)
        {
            armorAmountValue.SetBaseValue(newArmorAmount);
            UpdateArmorAdder();
        }

        public int GetArmorAmount()
        {
            return armorAmountValue;
        }

        protected override void OnTemplateSet()
        {
            base.OnTemplateSet();

            if (template is ArmorTemplate armorTemplate)
            {
                armorAmountValue.SetBaseValue(armorTemplate.armorAmount);
                UpdateArmorAdder();
            }
        }

        // 更新护甲添加组件设置
        private void UpdateArmorAdder()
        {
            var armorAdder = GetBehaviorComponent<ArmorAdderComponent>();
            if (armorAdder != null) armorAdder.SetArmorAmount(armorAmountValue);
        }

        protected override string FormatDescriptionInternal(string formattedDescription)
        {
            // 为轻甲道具添加特定的占位符替换
            return formattedDescription
                .Replace("{armor}", armorAmountValue.ToString());
        }
    }
}