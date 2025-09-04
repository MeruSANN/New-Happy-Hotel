using HappyHotel.Card;
using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.Description;
using HappyHotel.Core.Grid.Components;
using HappyHotel.Core.Registry;
using HappyHotel.Equipment.Templates;
using UnityEngine;
using HappyHotel.GameManager;

namespace HappyHotel.Prop
{
    [AutoInitComponent(typeof(GridObjectComponent))]
    public abstract class PropBase : BehaviorComponentContainer, ITypeIdSettable<PropTypeId>, IFormattableDescription
    {
        // 道具的基本属性
        protected ItemTemplate template;

        // 初始化状态标记
        private bool isInitialized;

        // 放置卡牌的引用（新增）
        private CardBase placedByCard;

        // 放置来源（仅卡牌）
        public enum PlacementSource
        {
            None,
            Card
        }

        private PlacementSource placementSource = PlacementSource.None;

        // 道具的类型ID
        public PropTypeId TypeId { get; private set; }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            PropManager.Instance?.Remove(this);
        }

        // 实现IFormattableDescription接口
        public virtual string GetDescriptionTemplate()
        {
            return template?.description ?? "";
        }

        public virtual string GetFormattedDescription()
        {
            var template = GetDescriptionTemplate();
            if (string.IsNullOrEmpty(template)) return "";

            return FormatDescriptionInternal(template);
        }

        // 实现ITypeIdSettable接口
        public void SetTypeId(PropTypeId typeId)
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
                var spriteRenderer = GetComponent<SpriteRenderer>();
                if (spriteRenderer != null) spriteRenderer.sprite = template.icon;
            }
        }

        // 设置放置卡牌的引用（新增）
        public void SetPlacedByCard(CardBase card)
        {
            placedByCard = card;
        }

        // 获取放置卡牌的引用（新增）
        public CardBase GetPlacedByCard()
        {
            return placedByCard;
        }

        // 设置放置来源为卡牌
        public void MarkPlacementByCard()
        {
            placementSource = PlacementSource.Card;
        }

        // 敏捷系统已移除

        // 获取放置来源
        public PlacementSource GetPlacementSource()
        {
            return placementSource;
        }

        // 道具被触发时的公共方法
        public virtual void OnTrigger(BehaviorComponentContainer triggerer)
        {
            // 检查是否已初始化
            if (!isInitialized)
            {
                Debug.LogWarning($"Prop {name} 尚未初始化，跳过触发");
                return;
            }

            // 调用子类的具体触发逻辑
            OnTriggerInternal(triggerer);

            // 发送Trigger事件给所有组件
            SendEvent(new BehaviorComponentEvent("Trigger", this, triggerer));
        }

        // 子类重写此方法来实现具体的触发逻辑
        public virtual void OnTriggerInternal(BehaviorComponentContainer triggerer)
        {
            // 默认实现为空，子类可以重写
        }

        // 标记Prop为已初始化
        public void MarkAsInitialized()
        {
            isInitialized = true;
            Debug.Log($"Prop {name} 已标记为初始化完成");
        }

        // 检查Prop是否已初始化
        public bool IsInitialized()
        {
            return isInitialized;
        }

        // 强制标记为未初始化（用于测试）
        public void MarkAsUninitialized()
        {
            isInitialized = false;
        }

        // 子类重写此方法来实现具体的格式化逻辑
        protected virtual string FormatDescriptionInternal(string formattedDescription)
        {
            return formattedDescription;
        }
    }
}