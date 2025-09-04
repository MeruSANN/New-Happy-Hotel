using HappyHotel.Core.Description;
using HappyHotel.Core.EntityComponent;
using HappyHotel.Core.Rarity;
using HappyHotel.Core.Registry;
using HappyHotel.Equipment.Templates;
using HappyHotel.Prop;
using UnityEngine;

namespace HappyHotel.Equipment
{
    // 背包中武器实例的基类
    public abstract class EquipmentBase : EntityComponentContainer, ITypeIdSettable<EquipmentTypeId>, IRarityProvider,
        IFormattableDescription
    {
        protected string baseEquipmentTypeId = "";

        // 是否为消耗型装备（从模板读取）
        protected bool isConsumable;

        // 升级相关属性
        protected bool isUpgradeable;
        protected bool isUpgradedEquipment;
        protected string upgradedEquipmentTypeId = "";

        // 构造函数
        public EquipmentBase() : base("Equipment")
        {
        }

        // 武器的类型ID
        public EquipmentTypeId TypeId { get; private set; }

        // 武器的模板数据
        public ItemTemplate Template { get; set; }

        public PropTypeId PropTypeId => Core.Registry.TypeId.Create<PropTypeId>(TypeId.ToString());

        // 检查是否为消耗型装备
        public bool IsConsumable => isConsumable;

        // 升级相关公共属性
        public bool IsUpgradeable => isUpgradeable;
        public string UpgradedEquipmentTypeId => upgradedEquipmentTypeId;
        public bool IsUpgradedEquipment => isUpgradedEquipment;
        public string BaseEquipmentTypeId => baseEquipmentTypeId;

        // 实现IFormattableDescription接口
        public virtual string GetDescriptionTemplate()
        {
            return Template?.description ?? "";
        }

        public virtual string GetFormattedDescription()
        {
            var template = GetDescriptionTemplate();
            if (string.IsNullOrEmpty(template))
                return "";

            // 替换通用占位符
            var formatted = template
                .Replace("{rarity}", Rarity.ToString());

            // 调用子类的自定义格式化
            return FormatDescriptionInternal(formatted);
        }

        // 武器稀有度
        public Rarity Rarity { get; protected set; } = Rarity.Common;

        // 实现ITypeIdSettable接口
        public void SetTypeId(EquipmentTypeId typeId)
        {
            TypeId = typeId;
        }

        public void SetTemplate(ItemTemplate newTemplate)
        {
            Template = newTemplate;
            Rarity = newTemplate.rarity;
            OnTemplateSet();
        }

        // 当配置被设置时调用
        protected virtual void OnTemplateSet()
        {
            // 从EquipmentTemplate读取isConsumable设置
            if (Template is EquipmentTemplate equipmentTemplate)
            {
                isConsumable = equipmentTemplate.isConsumable;

                // 读取升级相关设置
                isUpgradeable = equipmentTemplate.isUpgradeable;
                upgradedEquipmentTypeId = equipmentTemplate.upgradedEquipmentTypeId;
                isUpgradedEquipment = equipmentTemplate.isUpgradedEquipment;
                baseEquipmentTypeId = equipmentTemplate.baseEquipmentTypeId;

                // 加载模板中的Tags
                if (equipmentTemplate.tags != null)
                    foreach (var tag in equipmentTemplate.tags)
                        AddTag(tag);
            }
            else
            {
                // 如果不是EquipmentTemplate，默认为非消耗型和不可升级
                isConsumable = false;
                isUpgradeable = false;
                upgradedEquipmentTypeId = "";
                isUpgradedEquipment = false;
                baseEquipmentTypeId = "";
            }
        }

        // 在指定位置放置对应的Prop，默认实现不传递Setting参数
        public virtual PropBase PlaceProp(Vector2Int position)
        {
            // 不再传递PropSetting，数值将通过OnTemplateSet从Template中读取
            return PropController.Instance.PlaceProp(position, PropTypeId);
        }

        // 检查是否可以执行升级
        public virtual bool CanUpgrade()
        {
            return isUpgradeable &&
                   !isUpgradedEquipment &&
                   !string.IsNullOrEmpty(upgradedEquipmentTypeId);
        }

        // 获取升级后的装备TypeId
        public virtual EquipmentTypeId GetUpgradedEquipmentTypeId()
        {
            if (!CanUpgrade())
                return null;

            return Core.Registry.TypeId.Create<EquipmentTypeId>(upgradedEquipmentTypeId);
        }

        // 获取基础装备TypeId（如果这是升级后的装备）
        public virtual EquipmentTypeId GetBaseEquipmentTypeId()
        {
            if (!isUpgradedEquipment || string.IsNullOrEmpty(baseEquipmentTypeId))
                return null;

            return Core.Registry.TypeId.Create<EquipmentTypeId>(baseEquipmentTypeId);
        }

        // 子类可以重写此方法来添加自定义的占位符替换
        protected virtual string FormatDescriptionInternal(string formattedDescription)
        {
            return formattedDescription;
        }
    }
}