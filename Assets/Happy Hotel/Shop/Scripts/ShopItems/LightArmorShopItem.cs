using HappyHotel.Core.ValueProcessing;
using HappyHotel.Equipment.Templates;
using UnityEngine;

namespace HappyHotel.Shop
{
    // 购买后获得轻甲装备的商店道具
    public class LightArmorShopItem : EquipmentShopItemBase
    {
        // 轻甲护甲值
        [SerializeField] private EquipmentValue armorAmount = new("护甲值");

        public LightArmorShopItem()
        {
            armorAmount.Initialize(this);
        }

        public int ArmorAmount => armorAmount;

        public void SetArmorAmount(int newArmorAmount)
        {
            armorAmount.SetBaseValue(Mathf.Max(0, newArmorAmount));
        }

        protected override void OnTemplateSet()
        {
            base.OnTemplateSet();

            // 从轻甲商店道具模板中读取护甲值
            if (template is ArmorTemplate armorTemplate) armorAmount.SetBaseValue(armorTemplate.armorAmount);
        }

        public override bool CanAddToInventory()
        {
            // 检查基础条件
            if (!base.CanAddToInventory()) return false;

            // 这里我们允许拥有多个轻甲
            return true;
        }

        protected override string FormatDescriptionInternal(string formattedDescription)
        {
            // 为轻甲商店道具添加特定的占位符替换
            return formattedDescription
                .Replace("{armor}", armorAmount.ToString());
        }
    }
}