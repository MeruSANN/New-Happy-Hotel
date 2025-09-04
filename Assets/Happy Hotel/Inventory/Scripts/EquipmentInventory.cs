using System.Collections.Generic;
using System.Linq;
using HappyHotel.Core.Singleton;
using HappyHotel.Equipment;
using HappyHotel.Equipment.Settings;
using UnityEngine;

// Added for .Where() and .ToList()

namespace HappyHotel.Inventory
{
    // 只负责装备的存储、容量、增删查
    [ManagedSingleton(true)]
    public class EquipmentInventory : SingletonBase<EquipmentInventory>
    {
        public delegate void InventoryEvent(EquipmentTypeId equipmentTypeId);

        public delegate void InventoryFullEvent(EquipmentTypeId equipmentTypeId);

        private readonly Dictionary<EquipmentTypeId, int> destroyedEquipments = new(); // 销毁区
        private readonly List<EquipmentBase> equipments = new();
        private readonly Dictionary<EquipmentTypeId, int> refreshedEquipments = new(); // 已刷新区（弃牌区）

        // 区域存储
        private readonly Dictionary<EquipmentTypeId, int> unrefreshedEquipments = new(); // 未刷新区（抽牌区）
        // 在场区：记录当前已放置在场的数量（属于已刷新区的子集）
        private readonly Dictionary<EquipmentTypeId, int> inPlayEquipments = new();

        public int CurrentEquipmentCount => equipments.Count;

        public IReadOnlyList<EquipmentBase> Equipments => equipments.AsReadOnly();

        public IReadOnlyDictionary<EquipmentTypeId, int> UnrefreshedEquipments => unrefreshedEquipments;
        public IReadOnlyDictionary<EquipmentTypeId, int> RefreshedEquipments => refreshedEquipments;
        public IReadOnlyDictionary<EquipmentTypeId, int> DestroyedEquipments => destroyedEquipments;
        public IReadOnlyDictionary<EquipmentTypeId, int> InPlayEquipments => inPlayEquipments;

        public event InventoryEvent onEquipmentAdded;
        public event InventoryEvent onEquipmentRemoved;

        // 静默标志：为批量恢复提供事件抑制
        private bool suppressEvents;

        // 设置是否抑制事件（静默模式）
        public void SetEventSuppressed(bool suppressed)
        {
            suppressEvents = suppressed;
        }

        public bool AddEquipment(EquipmentTypeId equipmentTypeId, IEquipmentSetting setting = null)
        {
            if (equipmentTypeId == null)
            {
                Debug.LogWarning("尝试添加无效的装备类型ID");
                return false;
            }

            
            var equipment = EquipmentManager.Instance.Create(equipmentTypeId, setting);
            if (equipment == null)
            {
                Debug.LogError($"无法创建类型为 {equipmentTypeId.Id} 的装备");
                return false;
            }

            equipments.Add(equipment);
            // 新增：添加到未刷新区
            if (!unrefreshedEquipments.ContainsKey(equipmentTypeId))
                unrefreshedEquipments[equipmentTypeId] = 0;
            unrefreshedEquipments[equipmentTypeId]++;
            
            if (!suppressEvents) onEquipmentAdded?.Invoke(equipmentTypeId);
            
            return true;
        }

        public bool RemoveEquipment(EquipmentTypeId equipmentTypeId)
        {
            if (equipmentTypeId == null)
            {
                Debug.LogWarning("尝试移除无效的装备类型ID");
                return false;
            }

            
            var equipment = GetEquipmentByTypeId(equipmentTypeId);
            if (equipment != null)
            {
                equipments.Remove(equipment);
                // 新增：从所有区移除
                if (unrefreshedEquipments.ContainsKey(equipmentTypeId) && unrefreshedEquipments[equipmentTypeId] > 0)
                {
                    unrefreshedEquipments[equipmentTypeId]--;
                    if (unrefreshedEquipments[equipmentTypeId] == 0) unrefreshedEquipments.Remove(equipmentTypeId);
                }
                else if (refreshedEquipments.ContainsKey(equipmentTypeId) && refreshedEquipments[equipmentTypeId] > 0)
                {
                    refreshedEquipments[equipmentTypeId]--;
                    if (refreshedEquipments[equipmentTypeId] == 0) refreshedEquipments.Remove(equipmentTypeId);
                }
                else if (destroyedEquipments.ContainsKey(equipmentTypeId) && destroyedEquipments[equipmentTypeId] > 0)
                {
                    destroyedEquipments[equipmentTypeId]--;
                    if (destroyedEquipments[equipmentTypeId] == 0) destroyedEquipments.Remove(equipmentTypeId);
                }

                if (!suppressEvents) onEquipmentRemoved?.Invoke(equipmentTypeId);
                
                
                return true;
            }

            Debug.LogWarning($"尝试移除不存在的装备类型: {equipmentTypeId.Id}");
            return false;
        }

        public EquipmentBase GetEquipmentByTypeId(EquipmentTypeId typeId)
        {
            return equipments.Find(w => w.TypeId.Equals(typeId));
        }

        public IEnumerable<EquipmentBase> GetEquipmentsByTypeId(EquipmentTypeId typeId)
        {
            return equipments.FindAll(w => w.TypeId.Equals(typeId));
        }

        public bool HasEquipmentOfType(EquipmentTypeId typeId)
        {
            return equipments.Exists(w => w.TypeId.Equals(typeId));
        }

        // 标记装备为已刷新（未刷新区->已刷新区）
        public void MarkAsRefreshed(EquipmentTypeId equipmentTypeId)
        {
            if (equipmentTypeId == null) return;
            
            if (unrefreshedEquipments.ContainsKey(equipmentTypeId) && unrefreshedEquipments[equipmentTypeId] > 0)
            {
                unrefreshedEquipments[equipmentTypeId]--;
                if (unrefreshedEquipments[equipmentTypeId] == 0) unrefreshedEquipments.Remove(equipmentTypeId);
                if (!refreshedEquipments.ContainsKey(equipmentTypeId)) refreshedEquipments[equipmentTypeId] = 0;
                refreshedEquipments[equipmentTypeId]++;
                
            }
            
        }

        // 标记装备为销毁（未刷新区/已刷新区->销毁区）
        public void MarkAsDestroyed(EquipmentTypeId equipmentTypeId)
        {
            if (equipmentTypeId == null) return;
            
            var removed = false; // 是否从某个区成功移除

            // 修复：优先从已刷新区移除
            if (refreshedEquipments.ContainsKey(equipmentTypeId) && refreshedEquipments[equipmentTypeId] > 0)
            {
                refreshedEquipments[equipmentTypeId]--;
                if (refreshedEquipments[equipmentTypeId] == 0) refreshedEquipments.Remove(equipmentTypeId);
                removed = true;
                
            }
            else if (unrefreshedEquipments.ContainsKey(equipmentTypeId) && unrefreshedEquipments[equipmentTypeId] > 0)
            {
                unrefreshedEquipments[equipmentTypeId]--;
                if (unrefreshedEquipments[equipmentTypeId] == 0) unrefreshedEquipments.Remove(equipmentTypeId);
                removed = true;
                
            }

            if (!removed)
            {
                
                return;
            }

            if (!destroyedEquipments.ContainsKey(equipmentTypeId)) destroyedEquipments[equipmentTypeId] = 0;
            destroyedEquipments[equipmentTypeId]++;
            
            
        }

        // 重置已刷新区（弃牌区洗入抽牌区）
        public void ResetRefreshed()
        {
            
            var keys = refreshedEquipments.Keys.ToList();
            var movedTotal = 0;
            foreach (var key in keys)
            {
                var totalRefreshed = GetRefreshedCount(key);
                var inPlay = GetInPlayCount(key);
                var movable = Mathf.Max(0, totalRefreshed - inPlay);
                if (movable > 0)
                {
                    if (!unrefreshedEquipments.ContainsKey(key)) unrefreshedEquipments[key] = 0;
                    unrefreshedEquipments[key] += movable;
                    movedTotal += movable;
                    
                }

                var remainingRefreshed = Mathf.Min(inPlay, totalRefreshed);
                if (remainingRefreshed > 0)
                {
                    refreshedEquipments[key] = remainingRefreshed;
                }
                else
                {
                    refreshedEquipments.Remove(key);
                }
                
            }
            
            
        }

        // 重置销毁区（销毁区洗入未刷新区）
        public void ResetDestroyed()
        {
            
            var movedTotal = 0;
            foreach (var kv in destroyedEquipments)
            {
                if (!unrefreshedEquipments.ContainsKey(kv.Key)) unrefreshedEquipments[kv.Key] = 0;
                unrefreshedEquipments[kv.Key] += kv.Value;
                movedTotal += kv.Value;
                
                
            }

            destroyedEquipments.Clear();
            
            
        }

        // 重置所有区
        public void ResetAllZones()
        {
            ResetRefreshed();
            ResetDestroyed();
        }

        public void ClearInventory()
        {
            var equipmentsToRemove = new List<EquipmentBase>(equipments);
            foreach (var equipment in equipmentsToRemove) RemoveEquipment(equipment.TypeId);
            equipments.Clear();
            unrefreshedEquipments.Clear();
            refreshedEquipments.Clear();
            destroyedEquipments.Clear();
            inPlayEquipments.Clear();
        }


        // 新增：设置已刷新区某类型数量
        public void SetRefreshedCount(EquipmentTypeId typeId, int count)
        {
            if (typeId == null) return;
            if (count <= 0)
                refreshedEquipments.Remove(typeId);
            else
                refreshedEquipments[typeId] = count;
        }

        // 新增：将已刷新区某类型的多余数量转回未刷新区
        public void MoveRefreshedToUnrefreshed(EquipmentTypeId typeId, int count)
        {
            if (typeId == null || count <= 0) return;
            
            var refreshed = GetRefreshedCount(typeId);
            var inPlay = GetInPlayCount(typeId);
            var available = Mathf.Max(0, refreshed - inPlay);
            var moveCount = Mathf.Min(count, available);
            if (moveCount <= 0) return;
            refreshedEquipments[typeId] = refreshed - moveCount;
            if (refreshedEquipments[typeId] <= 0) refreshedEquipments.Remove(typeId);
            if (!unrefreshedEquipments.ContainsKey(typeId)) unrefreshedEquipments[typeId] = 0;
            unrefreshedEquipments[typeId] += moveCount;
            
            
        }

        // 新增：清空已刷新区某类型
        public void ClearRefreshedType(EquipmentTypeId typeId)
        {
            if (typeId == null) return;
            refreshedEquipments.Remove(typeId);
        }

        // 新增：三大区数量查询
        public int GetRefreshedCount(EquipmentTypeId typeId)
        {
            if (typeId == null) return 0;
            return refreshedEquipments.TryGetValue(typeId, out var v) ? v : 0;
        }

        public int GetUnrefreshedCount(EquipmentTypeId typeId)
        {
            if (typeId == null) return 0;
            return unrefreshedEquipments.TryGetValue(typeId, out var v) ? v : 0;
        }

        public int GetDestroyedCount(EquipmentTypeId typeId)
        {
            if (typeId == null) return 0;
            return destroyedEquipments.TryGetValue(typeId, out var v) ? v : 0;
        }

        // 在场区数量查询
        public int GetInPlayCount(EquipmentTypeId typeId)
        {
            if (typeId == null) return 0;
            return inPlayEquipments.TryGetValue(typeId, out var v) ? v : 0;
        }

        // 标记某类型装备放置到场上
        public void MarkAsDeployed(EquipmentTypeId typeId)
        {
            if (typeId == null) return;
            
            if (!inPlayEquipments.ContainsKey(typeId)) inPlayEquipments[typeId] = 0;
            inPlayEquipments[typeId]++;
            
            
        }

        // 标记某类型装备从场上移除
        public void MarkAsUndeployed(EquipmentTypeId typeId)
        {
            if (typeId == null) return;
            
            if (!inPlayEquipments.ContainsKey(typeId)) return;
            inPlayEquipments[typeId] = Mathf.Max(0, inPlayEquipments[typeId] - 1);
            if (inPlayEquipments[typeId] == 0) inPlayEquipments.Remove(typeId);
            
            
        }

        // 重置在场计数（通常在新回合开始前调用）
        public void ResetInPlayCounts()
        {
            inPlayEquipments.Clear();
        }

        // 新增：获取未刷新区总数
        public int GetUnrefreshedTotalCount()
        {
            var sum = 0;
            foreach (var kv in unrefreshedEquipments) sum += kv.Value;
            return sum;
        }

        // 新增：获取已刷新区总数
        public int GetRefreshedTotalCount()
        {
            var sum = 0;
            foreach (var kv in refreshedEquipments) sum += kv.Value;
            return sum;
        }

        // 新增：获取销毁区总数
        public int GetDestroyedTotalCount()
        {
            var sum = 0;
            foreach (var kv in destroyedEquipments) sum += kv.Value;
            return sum;
        }

        // 升级系统相关查询方法

        // 获取所有可升级的装备列表
        public IReadOnlyList<EquipmentBase> GetUpgradeableEquipments()
        {
            return equipments.Where(equipment => equipment.CanUpgrade()).ToList().AsReadOnly();
        }

        // 获取所有已升级的装备列表
        public IReadOnlyList<EquipmentBase> GetUpgradedEquipments()
        {
            return equipments.Where(equipment => equipment.IsUpgradedEquipment).ToList().AsReadOnly();
        }

        // 获取所有未升级的装备列表（排除已升级的装备）
        public IReadOnlyList<EquipmentBase> GetNonUpgradedEquipments()
        {
            return equipments.Where(equipment => !equipment.IsUpgradedEquipment).ToList().AsReadOnly();
        }

        // 获取所有基础装备列表（可升级 + 不可升级但非升级版本）
        public IReadOnlyList<EquipmentBase> GetBaseEquipments()
        {
            return equipments.Where(equipment => !equipment.IsUpgradedEquipment).ToList().AsReadOnly();
        }

        // 检查是否有指定类型的可升级装备
        public bool HasUpgradeableEquipment(EquipmentTypeId equipmentTypeId)
        {
            if (equipmentTypeId == null) return false;
            var equipment = GetEquipmentByTypeId(equipmentTypeId);
            return equipment?.CanUpgrade() ?? false;
        }

        // 检查是否有指定类型的已升级装备
        public bool HasUpgradedEquipment(EquipmentTypeId equipmentTypeId)
        {
            if (equipmentTypeId == null) return false;
            var equipment = GetEquipmentByTypeId(equipmentTypeId);
            return equipment?.IsUpgradedEquipment ?? false;
        }

        // 统计可升级装备数量
        public int GetUpgradeableEquipmentCount()
        {
            return equipments.Count(equipment => equipment.CanUpgrade());
        }

        // 统计已升级装备数量
        public int GetUpgradedEquipmentCount()
        {
            return equipments.Count(equipment => equipment.IsUpgradedEquipment);
        }

        // 统计基础装备数量（非升级版本的装备）
        public int GetBaseEquipmentCount()
        {
            return equipments.Count(equipment => !equipment.IsUpgradedEquipment);
        }

        // 获取未刷新区的装备实例列表
        public List<EquipmentBase> GetUnrefreshedEquipmentInstances()
        {
            var result = new List<EquipmentBase>();
            foreach (var kv in unrefreshedEquipments)
            {
                var all = GetEquipmentsByTypeId(kv.Key).ToList();
                for (var i = 0; i < kv.Value && i < all.Count; i++)
                    result.Add(all[i]);
            }

            return result;
        }

        // 获取已刷新区的装备实例列表
        public List<EquipmentBase> GetRefreshedEquipmentInstances()
        {
            var result = new List<EquipmentBase>();
            foreach (var kv in refreshedEquipments)
            {
                var all = GetEquipmentsByTypeId(kv.Key).ToList();
                var start = unrefreshedEquipments.TryGetValue(kv.Key, out var unrefreshed) ? unrefreshed : 0;
                for (var i = start; i < start + kv.Value && i < all.Count; i++)
                    result.Add(all[i]);
            }

            return result;
        }

        // 获取销毁区的装备实例列表
        public List<EquipmentBase> GetDestroyedEquipmentInstances()
        {
            var result = new List<EquipmentBase>();
            foreach (var kv in destroyedEquipments)
            {
                var all = GetEquipmentsByTypeId(kv.Key).ToList();
                var start = 0;
                if (unrefreshedEquipments.TryGetValue(kv.Key, out var unrefreshed)) start += unrefreshed;
                if (refreshedEquipments.TryGetValue(kv.Key, out var refreshed)) start += refreshed;
                for (var i = start; i < start + kv.Value && i < all.Count; i++)
                    result.Add(all[i]);
            }

            return result;
        }
    }
}