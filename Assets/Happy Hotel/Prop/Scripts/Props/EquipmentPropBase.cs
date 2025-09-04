using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Equipment;
using HappyHotel.Equipment.Templates;
using HappyHotel.Inventory;

namespace HappyHotel.Prop
{
    // 装备类Prop基类，由装备生成的Prop
    public abstract class EquipmentPropBase : PropBase
    {
        // 是否为消耗型装备（从模板读取）
        protected bool isConsumableEquipment;

        // 是否为单件装备（从模板读取）
        protected bool isSingleUseEquipment;

        // 当模板被设置时调用
        protected override void OnTemplateSet()
        {
            base.OnTemplateSet();

            // 从EquipmentTemplate读取设置
            if (template is EquipmentTemplate equipmentTemplate)
            {
                isConsumableEquipment = equipmentTemplate.isConsumable;
                isSingleUseEquipment = equipmentTemplate.isSingleUse;

                // 加载模板中的Tags
                if (equipmentTemplate.tags != null)
                    foreach (var tag in equipmentTemplate.tags)
                        AddTag(tag);
            }
            else
            {
                // 如果不是EquipmentTemplate，默认为非消耗型和非单件型
                isConsumableEquipment = false;
                isSingleUseEquipment = false;
            }
        }

        // 重写OnTrigger方法，添加消耗型装备和单件装备处理逻辑
        public override void OnTrigger(BehaviorComponentContainer triggerer)
        {
            base.OnTrigger(triggerer);
            HandleEquipmentLogic();
        }

        // 道具被触发时调用（处理消耗型装备和单件装备逻辑）
        protected virtual void HandleEquipmentLogic()
        {
            // 如果是消耗型装备，标记为已使用并销毁
            if (isConsumableEquipment && template != null && !string.IsNullOrEmpty(TypeId.ToString()))
            {
                var equipmentTypeId = Core.Registry.TypeId.Create<EquipmentTypeId>(TypeId.ToString());
                EquipmentSpawnManager.Instance?.MarkEquipmentAsUsed(equipmentTypeId);
                Destroy(gameObject);
            }
            else
            {
                // 非消耗路径：从在场区移除计数，并销毁当前道具（单件或普通非消耗）
                if (template != null && !string.IsNullOrEmpty(TypeId.ToString()))
                {
                    var equipmentTypeId = Core.Registry.TypeId.Create<EquipmentTypeId>(TypeId.ToString());
                    EquipmentInventory.Instance?.MarkAsUndeployed(equipmentTypeId);
                }

                // 如果是单件装备，直接销毁；普通非消耗也销毁以便下回合可再次刷新
                Destroy(gameObject);
            }
        }
    }
}