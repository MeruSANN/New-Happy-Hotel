using HappyHotel.Core.Description;
using HappyHotel.Core.EntityComponent;
using HappyHotel.Core.Registry;
using HappyHotel.Equipment.Templates;
using HappyHotel.Inventory;
using UnityEngine;

namespace HappyHotel.Card
{
    // 卡牌基类，所有卡牌类型的父类
    public abstract class CardBase : EntityComponentContainer, ITypeIdSettable<CardTypeId>, IFormattableDescription
    {
        // 是否为消耗型卡牌（从模板读取）
        protected bool isConsumable;

        // 卡牌的模板数据
        protected CardTemplate template;

        // 构造函数
        protected CardBase() : base("CardBase")
        {
        }

        // 卡牌的类型ID
        public CardTypeId TypeId { get; private set; }

        // 公共属性，用于获取卡牌模板
        public CardTemplate Template => template;

        // 检查是否为消耗型卡牌
        public bool IsConsumable => isConsumable;

        // 实现IFormattableDescription接口
        public virtual string GetDescriptionTemplate()
        {
            return template?.description ?? "";
        }

        public virtual string GetFormattedDescription()
        {
            var template = GetDescriptionTemplate();
            if (string.IsNullOrEmpty(template))
                return "";

            // 调用子类的自定义格式化
            return FormatDescriptionInternal(template);
        }

        // 实现ITypeIdSettable接口
        public void SetTypeId(CardTypeId typeId)
        {
            TypeId = typeId;
        }

        public void SetTemplate(CardTemplate newTemplate)
        {
            template = newTemplate;
            OnTemplateSet();
        }

        // 当模板被设置时调用
        protected virtual void OnTemplateSet()
        {
            // 从CardTemplate读取isConsumable设置
            if (template != null)
                isConsumable = template.isConsumable;
            else
                // 如果模板为空，默认为非消耗型
                isConsumable = false;
        }

        // 使用卡牌的方法，用于不需要选择位置的卡牌
        public virtual bool UseCard()
        {
            // 发送UseCard事件到所有EntityComponent
            var useCardEvent = new EntityComponentEvent("UseCard", this);
            SendEvent(useCardEvent);

            // 处理消耗逻辑
            HandleConsumableLogic();

            // 默认实现返回true，表示事件已发送
            // 子类可以重写此方法来实现额外的逻辑
            return true;
        }

        // 处理消耗卡牌逻辑
        protected virtual void HandleConsumableLogic()
        {
            // ActivePlacementCard及其子类不在这里处理消耗逻辑，它们会移到临时区
            if (this is ActivePlacementCard)
                // ActivePlacementCard的消耗逻辑由子类自己处理
                return;

            // 如果CardDrawManager存在，让它处理库存管理
            if (CardDrawManager.Instance != null)
                CardDrawManager.Instance.HandleCardConsumption(this);
            else
                Debug.LogWarning("[CardBase] HandleConsumableLogic: CardDrawManager不存在，无法处理卡牌消耗逻辑");
        }

        // 子类可以重写此方法来添加自定义的占位符替换
        protected virtual string FormatDescriptionInternal(string template)
        {
            return template;
        }
    }
}