using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.Description;
using HappyHotel.Core.Rarity;
using HappyHotel.Core.Registry;
using HappyHotel.Equipment.Templates;
using UnityEngine;

namespace HappyHotel.Shop
{
    // 商店道具基类，定义价格属性和购买方法
    public abstract class ShopItemBase : BehaviorComponentContainer, ITypeIdSettable<ShopItemTypeId>, IRarityProvider,
        IFormattableDescription
    {
        // 道具的基本属性
        protected ItemTemplate template;

        // 道具名称
        protected string itemName;

        // 道具描述
        protected string description;

        // 道具图标
        protected Sprite itemIcon;

        // 道具稀有度
        protected Rarity rarity = Rarity.Common;

        // 道具的类型ID
        public ShopItemTypeId TypeId { get; private set; }

        // 属性访问器
        public int Price { get; private set; } // 只读属性，由OnTemplateSet设置
        public string ItemName => itemName;
        public string Description => description;
        public Sprite ItemIcon => itemIcon;

        // 实现IFormattableDescription接口
        public virtual string GetDescriptionTemplate()
        {
            return description ?? "";
        }

        public virtual string GetFormattedDescription()
        {
            var template = GetDescriptionTemplate();
            if (string.IsNullOrEmpty(template))
                return "";

            // 调用子类的自定义格式化
            return FormatDescriptionInternal(template);
        }

        public Rarity Rarity => rarity;

        // 实现ITypeIdSettable接口
        public void SetTypeId(ShopItemTypeId typeId)
        {
            TypeId = typeId;
        }

        public void SetTemplate(ItemTemplate newTemplate)
        {
            template = newTemplate;
            OnTemplateSet();
        }

        // 当模板被设置时调用
        protected virtual void OnTemplateSet()
        {
            if (template != null)
            {
                itemName = template.itemName;
                description = template.description;
                itemIcon = template.icon;
                rarity = template.rarity;
            }
        }

        public void GeneratePrice()
        {
            // 通过ShopController获取价格配置
            var shopController = ShopController.Instance;
            if (shopController == null)
            {
                Debug.LogError("[ShopItemBase] ShopController.Instance 为 null，无法获取价格配置");
                Price = 0;
                return;
            }

            // 获取带浮动的价格
            Price = shopController.GetRandomizedPrice(Rarity);
        }

        // 购买方法 - 检查是否可以购买
        public virtual bool CanPurchase(int playerMoney)
        {
            return playerMoney >= Price;
        }

        // 购买方法 - 执行购买逻辑
        public virtual bool Purchase(int playerMoney)
        {
            if (!CanPurchase(playerMoney))
            {
                Debug.LogWarning($"无法购买道具 {itemName}：金钱不足或道具不可购买");
                return false;
            }

            // 调用子类的具体购买逻辑
            OnPurchase();

            Debug.Log($"成功购买道具 {itemName}，花费 {Price} 金币");
            return true;
        }

        // 子类重写此方法来实现具体的购买逻辑
        protected virtual void OnPurchase()
        {
            // 默认实现为空，子类可以重写
        }

        // 执行购买逻辑（供奖励系统使用，无需金币检查）
        public virtual void ExecutePurchaseLogic()
        {
            OnPurchase();
            Debug.Log($"已执行道具获取逻辑: {itemName}");
        }


        // 设置稀有度
        public virtual void SetRarity(Rarity newRarity)
        {
            rarity = newRarity;
        }

        // 子类可以重写此方法来添加自定义的占位符替换
        protected virtual string FormatDescriptionInternal(string formattedDescription)
        {
            return formattedDescription;
        }
    }
}