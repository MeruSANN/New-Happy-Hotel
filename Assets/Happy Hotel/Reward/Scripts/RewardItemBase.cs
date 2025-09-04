using HappyHotel.Core.Description;
using HappyHotel.Core.Rarity;
using HappyHotel.Core.Registry;
using HappyHotel.Reward.Templates;
using UnityEngine;

namespace HappyHotel.Reward
{
    // 奖励物品基类，定义奖励物品的基本属性和执行方法
    public abstract class RewardItemBase : MonoBehaviour, ITypeIdSettable<RewardItemTypeId>, IRarityProvider,
        IFormattableDescription
    {
        // 物品名称
        [SerializeField] protected string itemName;

        // 物品描述
        [SerializeField] protected string description;

        // 物品图标
        [SerializeField] protected Sprite itemIcon;

        // 物品稀有度
        [SerializeField] protected Rarity rarity = Rarity.Common;

        // 物品的基本属性
        protected RewardItemTemplate template;

        // 物品的类型ID
        public RewardItemTypeId TypeId { get; private set; }

        // 属性访问器
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
        public void SetTypeId(RewardItemTypeId typeId)
        {
            TypeId = typeId;
        }

        public void SetTemplate(RewardItemTemplate newTemplate)
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

        // 执行奖励逻辑
        public virtual bool Execute()
        {
            Debug.Log($"执行奖励物品: {itemName}");
            return OnExecute();
        }

        // 子类重写此方法来实现具体的奖励逻辑
        // 返回true表示立即完成，返回false表示等待用户选择
        protected virtual bool OnExecute()
        {
            // 默认实现为空，子类可以重写
            return true; // 默认立即完成
        }

        // 添加选择完成方法，供子类调用
        protected void CompleteSelection()
        {
            Debug.Log($"RewardItemBase: CompleteSelection被调用，奖励物品={itemName}");

            // 通知 RewardClaimController 完成选择
            if (RewardClaimController.Instance != null)
            {
                Debug.Log("RewardItemBase: 通知RewardClaimController完成选择");
                RewardClaimController.Instance.OnRewardItemSelectionCompleted(this);
            }
            else
            {
                Debug.LogError("RewardItemBase: RewardClaimController.Instance为null，无法通知完成选择");
            }
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