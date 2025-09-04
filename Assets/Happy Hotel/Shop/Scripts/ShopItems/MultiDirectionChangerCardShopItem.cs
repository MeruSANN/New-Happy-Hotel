using HappyHotel.Core.ValueProcessing;
using HappyHotel.Equipment.Templates;
using UnityEngine;

namespace HappyHotel.Shop
{
    // 购买后获得多重转向器卡牌的商店道具
    public class MultiDirectionChangerCardShopItem : CardShopItemBase
    {
        // 最大触发次数
        [SerializeField] private EquipmentValue maxTriggerCount = new("最大触发次数");

        public MultiDirectionChangerCardShopItem()
        {
            maxTriggerCount.Initialize(this);
        }

        public int MaxTriggerCount => maxTriggerCount;

        public void SetMaxTriggerCount(int newMaxTriggerCount)
        {
            maxTriggerCount.SetBaseValue(Mathf.Max(1, newMaxTriggerCount));
        }

        protected override void OnTemplateSet()
        {
            base.OnTemplateSet();
            // 从多重转向器卡牌模板中读取最大触发次数
            if (template is CardTemplate cardTemplate)
            {
                var maxTriggerCountField = template.GetType().GetField("maxTriggerCount");
                if (maxTriggerCountField != null)
                {
                    var amount = (int)maxTriggerCountField.GetValue(template);
                    maxTriggerCount.SetBaseValue(amount);
                }
            }
        }

        protected override string FormatDescriptionInternal(string formattedDescription)
        {
            // 为多重转向器卡牌商店道具添加特定的占位符替换
            return formattedDescription
                .Replace("{maxTriggerCount}", maxTriggerCount.ToString());
        }
    }
}