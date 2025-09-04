using System;
using System.Collections.Generic;
using System.Linq;
using HappyHotel.Core.Singleton;
using HappyHotel.Inventory;
using UnityEngine;

namespace HappyHotel.Equipment
{
    // 装备升级服务，负责处理装备升级逻辑
    [ManagedSingleton(true)]
    public class EquipmentUpgradeService : SingletonBase<EquipmentUpgradeService>
    {
        // 升级事件
        public Action<EquipmentTypeId, EquipmentTypeId> onEquipmentUpgraded; // 原装备TypeId, 升级后装备TypeId
        public Action<EquipmentTypeId> onEquipmentUpgradeFailed; // 升级失败的装备TypeId

        // 尝试升级指定的装备
        public bool TryUpgradeEquipment(EquipmentTypeId equipmentTypeId)
        {
            if (equipmentTypeId == null)
            {
                Debug.LogWarning("尝试升级无效的装备类型ID");
                return false;
            }

            // 检查背包中是否有该装备
            var inventory = EquipmentInventory.Instance;
            if (inventory == null)
            {
                Debug.LogError("EquipmentInventory未初始化，无法进行装备升级");
                return false;
            }

            var equipment = inventory.GetEquipmentByTypeId(equipmentTypeId);
            if (equipment == null)
            {
                Debug.LogWarning($"背包中没有找到装备: {equipmentTypeId.Id}");
                onEquipmentUpgradeFailed?.Invoke(equipmentTypeId);
                return false;
            }

            // 检查装备是否可以升级
            if (!equipment.CanUpgrade())
            {
                Debug.LogWarning($"装备 {equipmentTypeId.Id} 不可升级");
                onEquipmentUpgradeFailed?.Invoke(equipmentTypeId);
                return false;
            }

            var upgradedTypeId = equipment.GetUpgradedEquipmentTypeId();
            if (upgradedTypeId == null)
            {
                Debug.LogError($"装备 {equipmentTypeId.Id} 的升级类型ID无效");
                onEquipmentUpgradeFailed?.Invoke(equipmentTypeId);
                return false;
            }

            // 执行升级
            return PerformUpgrade(equipmentTypeId, upgradedTypeId);
        }

        // 执行升级操作
        private bool PerformUpgrade(EquipmentTypeId originalTypeId, EquipmentTypeId upgradedTypeId)
        {
            var inventory = EquipmentInventory.Instance;
            var equipmentManager = EquipmentManager.Instance;

            if (inventory == null || equipmentManager == null)
            {
                Debug.LogError("必要的管理器未初始化，无法执行升级");
                return false;
            }

            // 创建升级后的装备
            var upgradedEquipment = equipmentManager.Create(upgradedTypeId);
            if (upgradedEquipment == null)
            {
                Debug.LogError($"无法创建升级后的装备: {upgradedTypeId.Id}");
                return false;
            }

            // 从背包中移除原装备
            if (!inventory.RemoveEquipment(originalTypeId))
            {
                Debug.LogError($"无法从背包中移除原装备: {originalTypeId.Id}");
                return false;
            }

            // 将升级后的装备添加到背包
            if (!inventory.AddEquipment(upgradedTypeId))
            {
                Debug.LogError($"无法将升级后的装备添加到背包: {upgradedTypeId.Id}");

                // 升级失败，尝试恢复原装备
                inventory.AddEquipment(originalTypeId);
                return false;
            }

            Debug.Log($"装备升级成功: {originalTypeId.Id} → {upgradedTypeId.Id}");
            onEquipmentUpgraded?.Invoke(originalTypeId, upgradedTypeId);

            return true;
        }

        // 检查装备是否可以升级
        public bool CanUpgradeEquipment(EquipmentTypeId equipmentTypeId)
        {
            if (equipmentTypeId == null)
                return false;

            var inventory = EquipmentInventory.Instance;
            if (inventory == null)
                return false;

            var equipment = inventory.GetEquipmentByTypeId(equipmentTypeId);
            return equipment?.CanUpgrade() ?? false;
        }

        // 获取装备的升级后类型ID
        public EquipmentTypeId GetUpgradedTypeId(EquipmentTypeId equipmentTypeId)
        {
            if (equipmentTypeId == null)
                return null;

            var inventory = EquipmentInventory.Instance;
            if (inventory == null)
                return null;

            var equipment = inventory.GetEquipmentByTypeId(equipmentTypeId);
            return equipment?.GetUpgradedEquipmentTypeId();
        }

        // 获取所有可升级的装备列表
        public List<EquipmentBase> GetUpgradeableEquipments()
        {
            var inventory = EquipmentInventory.Instance;
            if (inventory == null)
                return new List<EquipmentBase>();

            return inventory.Equipments
                .Where(equipment => equipment.CanUpgrade())
                .ToList();
        }

        // 获取所有已升级的装备列表
        public List<EquipmentBase> GetUpgradedEquipments()
        {
            var inventory = EquipmentInventory.Instance;
            if (inventory == null)
                return new List<EquipmentBase>();

            return inventory.Equipments
                .Where(equipment => equipment.IsUpgradedEquipment)
                .ToList();
        }

        // 获取所有未升级的装备列表（排除已升级的装备）
        public List<EquipmentBase> GetNonUpgradedEquipments()
        {
            var inventory = EquipmentInventory.Instance;
            if (inventory == null)
                return new List<EquipmentBase>();

            return inventory.Equipments
                .Where(equipment => !equipment.IsUpgradedEquipment)
                .ToList();
        }

        // 检查装备是否为升级版本
        public bool IsUpgradedEquipment(EquipmentTypeId equipmentTypeId)
        {
            if (equipmentTypeId == null)
                return false;

            var inventory = EquipmentInventory.Instance;
            if (inventory == null)
                return false;

            var equipment = inventory.GetEquipmentByTypeId(equipmentTypeId);
            return equipment?.IsUpgradedEquipment ?? false;
        }

        // 获取装备的基础版本TypeId（如果是升级版本）
        public EquipmentTypeId GetBaseEquipmentTypeId(EquipmentTypeId equipmentTypeId)
        {
            if (equipmentTypeId == null)
                return null;

            var inventory = EquipmentInventory.Instance;
            if (inventory == null)
                return null;

            var equipment = inventory.GetEquipmentByTypeId(equipmentTypeId);
            return equipment?.GetBaseEquipmentTypeId();
        }
    }
}