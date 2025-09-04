using HappyHotel.Equipment;
using HappyHotel.Equipment.Templates;
using HappyHotel.GameManager;
using HappyHotel.Inventory;
using UnityEngine;

namespace HappyHotel.Shop
{
    // 向背包添加物品的商店道具基类
    public abstract class EquipmentShopItemBase : ShopItemBase
    {
        protected override void OnPurchase()
        {
            base.OnPurchase();

            // 检查背包是否存在
            if (EquipmentInventory.Instance == null)
            {
                Debug.LogError("EquipmentInventory未初始化，无法添加物品到背包");
                return;
            }

            // 调用父类的通用添加逻辑
            var addResult = AddToInventory();

            if (addResult)
            {
                Debug.Log($"成功将 {itemName} 添加到背包");

                // 如果是唯一物品，标记为已获得
                if (template != null && template.isUnique)
                {
                    var uniqueItemManager = UniqueItemManager.Instance;
                    if (uniqueItemManager != null)
                    {
                        uniqueItemManager.MarkItemAsObtained(TypeId.Id);
                        Debug.Log($"标记唯一物品为已获得: {itemName} ({TypeId.Id})");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"无法将 {itemName} 添加到背包，可能背包已满或物品无效");
            }
        }

        // 通用的背包添加逻辑，由父类实现
        protected bool AddToInventory()
        {
            // 直接使用 ShopItem 的 TypeId 字符串创建对应的 Equipment TypeId
            var equipmentTypeId = Core.Registry.TypeId.Create<EquipmentTypeId>(TypeId.Id);
            if (equipmentTypeId == null)
            {
                Debug.LogError($"无法创建装备TypeId: {TypeId.Id}");
                return false;
            }

            // 不再创建装备设置，直接传递null
            // 数值将通过OnTemplateSet从Template中读取
            var addResult = EquipmentInventory.Instance.AddEquipment(equipmentTypeId);

            if (addResult)
                Debug.Log($"成功添加装备到背包: {equipmentTypeId.Id}");
            else
                Debug.LogWarning($"添加装备到背包失败: {equipmentTypeId.Id}");

            return addResult;
        }

        // 当模板被设置时调用
        protected override void OnTemplateSet()
        {
            base.OnTemplateSet();

            // 从EquipmentTemplate加载Tags
            if (template is EquipmentTemplate equipmentTemplate)
                if (equipmentTemplate.tags != null)
                    foreach (var tag in equipmentTemplate.tags)
                        AddTag(tag);
        }

        // 检查是否可以添加到背包（子类可以重写）
        public virtual bool CanAddToInventory()
        {
            if (EquipmentInventory.Instance == null) return false;
            // 直接允许添加
            return true;
        }
    }
}