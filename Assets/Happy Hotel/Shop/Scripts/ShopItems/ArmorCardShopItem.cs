using HappyHotel.Core.ValueProcessing;
using HappyHotel.Equipment.Templates;
using UnityEngine;

namespace HappyHotel.Shop
{
    // 购买后获得护甲卡牌的商店道具
    public class ArmorCardShopItem : CardShopItemBase
    {
        // 护甲数量
        [SerializeField] private EquipmentValue armorAmount = new("护甲数量");

        public ArmorCardShopItem()
        {
            armorAmount.Initialize(this);
        }

        public int ArmorAmount => armorAmount;

        public void SetArmorAmount(int newArmorAmount)
        {
            armorAmount.SetBaseValue(Mathf.Max(1, newArmorAmount));
        }

        protected override void OnTemplateSet()
        {
            base.OnTemplateSet();
            // 从护甲卡牌模板中读取护甲数量
            if (template is CardTemplate cardTemplate)
            {
                // 这里需要根据具体的护甲卡牌模板类型来获取护甲数量
                // 由于模板是ScriptableObject，我们需要通过反射或其他方式获取
                var armorAmountField = template.GetType().GetField("armorAmount");
                if (armorAmountField != null)
                {
                    var amount = (int)armorAmountField.GetValue(template);
                    armorAmount.SetBaseValue(amount);
                }
            }
        }

        protected override string FormatDescriptionInternal(string formattedDescription)
        {
            // 为护甲卡牌商店道具添加特定的占位符替换
            return formattedDescription
                .Replace("{armor}", armorAmount.ToString());
        }
    }
}