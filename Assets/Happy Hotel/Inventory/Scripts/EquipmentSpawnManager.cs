using System;
using System.Collections.Generic;
using System.Linq;
using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.Grid;
using HappyHotel.Core.Grid.Components;
using HappyHotel.Core.Singleton;
using HappyHotel.Equipment;
using HappyHotel.GameManager;
using HappyHotel.Prop;
using UnityEngine;
using Random = System.Random;

namespace HappyHotel.Inventory
{
    // 装备生成管理器，管理消耗型装备使用状态和装备刷新机制
    [ManagedSingleton(SceneLoadMode.Exclude, "MapEditScene", "ShopScene", "MainMenu")]
    [SingletonInitializationDependency(typeof(ConfigProvider))]
    public class EquipmentSpawnManager : SingletonBase<EquipmentSpawnManager>
    {
        private GameConfig gameConfig;
        public System.Action onAllConsumableEquipmentsReset; // 所有消耗型装备状态重置时触发
        public System.Action onAllStatesReset; // 所有状态重置时触发

        public Action<EquipmentTypeId> onEquipmentMarkedAsSpawned; // 装备被标记为已刷新时触发

        // 事件定义
        public Action<EquipmentTypeId> onEquipmentMarkedAsUsed; // 装备被标记为已使用时触发
        public System.Action onSpawnedEquipmentsReset; // 弃牌区重置时触发

        

        protected override void OnDestroy()
        {
            base.OnDestroy();
            TurnManager.onPlayerTurnStart -= OnTurnStart;
        }

        protected override void OnSingletonAwake()
        {
            base.OnSingletonAwake();
            LoadGameConfig();
            TurnManager.onPlayerTurnStart += OnTurnStart;
        }

        private void OnTurnStart(int turnNumber)
        {
            // 新回合开始：不做 R→U 重置，留待抽取阶段在 U 为空时再按需重置
            GeneratePropsFromInventory();
        }

        // 生成道具的主逻辑
        public void GeneratePropsFromInventory()
        {
            if (WaveSpawnManager.Instance != null && WaveSpawnManager.Instance.IsAllWavesCleared)
            {
                Debug.Log("敌人已清空，跳过装备刷新");
                return;
            }
                
            // 获取玩家背包所有武器
            var inventory = EquipmentInventory.Instance;
            if (inventory == null) return;
            var weapons = inventory.Equipments;
            if (weapons == null || weapons.Count == 0) return;

            // 过滤掉已使用的消耗型装备
            var availableWeapons = weapons.Where(weapon =>
            {
                if (weapon == null) return false;

                // 如果是消耗型装备且已被使用，则不生成
                if (weapon.IsConsumable && IsEquipmentUsed(weapon.TypeId)) return false;

                // 非消耗型装备或未使用的消耗型装备都可以生成
                return true;
            }).ToList();

            if (availableWeapons.Count == 0) return;

            

            // 获取道具刷新数量上限配置（代表场上可存在的装备数量上限）
            var maxSpawnCount = gameConfig != null ? gameConfig.MaxPropSpawnCount : 10;

            // 计算当前场上已有的装备数量
            var currentEquipmentCount = PropController.Instance?.GetEquipmentPropCount() ?? 0;
            

            // 计算需要刷新的装备数量
            var needToSpawnCount = Mathf.Max(0, maxSpawnCount - currentEquipmentCount);
            if (needToSpawnCount == 0) return;

            // 使用装备生成管理器选择要刷新的装备
            availableWeapons = SelectEquipmentsToSpawn(availableWeapons, needToSpawnCount);
            

            // 首先尝试获取四周都没有其他对象的合法位置
            var preferredPositions = SpawnPositionValidator.GetValidEquipmentSpawnPositions();

            // 如果优先位置不够，则使用普通合法位置作为备选
            var mainCharacter = GameObject.FindGameObjectWithTag("MainCharacter");
            var fallbackPositions = new List<Vector2Int>();
            if (preferredPositions.Count < availableWeapons.Count)
            {
                fallbackPositions = SpawnPositionValidator.GetValidSpawnPositions();
                // 排除玩家周围的位置
                if (mainCharacter)
                {
                    var container = mainCharacter.GetComponent<BehaviorComponentContainer>();
                    var gridComp = container ? container.GetBehaviorComponent<GridObjectComponent>() : null;
                    if (gridComp != null)
                    {
                        var playerPos = gridComp.GetGridPosition();
                        var exclude = new HashSet<Vector2Int>
                        {
                            playerPos + Vector2Int.up,
                            playerPos + Vector2Int.down,
                            playerPos + Vector2Int.left,
                            playerPos + Vector2Int.right
                        };
                        fallbackPositions = fallbackPositions.Where(p => !exclude.Contains(p)).ToList();
                    }
                }
            }

            if (preferredPositions.Count == 0 && fallbackPositions.Count == 0)
            {
                Debug.LogWarning("没有找到任何合法的刷新位置");
                return;
            }

            // 随机化位置列表
            var shuffledPreferredPositions = ShufflePositions(preferredPositions);
            var shuffledFallbackPositions = ShufflePositions(fallbackPositions);

            // 随机分配位置并生成道具
            var posIndex = 0;
            foreach (var weapon in availableWeapons)
            {
                if (weapon?.PropTypeId == null) continue;

                Vector2Int spawnPos;
                
                // 优先使用四周都没有其他对象的位置
                if (posIndex < shuffledPreferredPositions.Count)
                {
                    spawnPos = shuffledPreferredPositions[posIndex];
                }
                else
                {
                    // 如果优先位置不够，使用备选位置
                    var fallbackIndex = posIndex - shuffledPreferredPositions.Count;
                    if (fallbackIndex < shuffledFallbackPositions.Count)
                    {
                        spawnPos = shuffledFallbackPositions[fallbackIndex];
                    }
                    else
                    {
                        Debug.LogWarning($"没有足够的位置来放置装备 {weapon.Name}");
                        continue;
                    }
                }

                // 使用装备的PlaceProp方法创建对应的Prop
                var typeId = weapon.TypeId;
                
                weapon.PlaceProp(spawnPos);
                EquipmentInventory.Instance.MarkAsDeployed(typeId);
                
                posIndex++;
            }

            
        }

        private void LoadGameConfig()
        {
            try
            {
                var provider = ConfigProvider.Instance;
                gameConfig = provider ? provider.GetGameConfig() : null;
                if (gameConfig == null)
                    Debug.LogWarning("EquipmentSpawnManager: 无法通过ConfigProvider获取GameConfig，将使用默认值");
            }
            catch (Exception e)
            {
                Debug.LogError($"EquipmentSpawnManager: 通过ConfigProvider加载配置时发生异常: {e.Message}");
            }
        }

        // 标记装备为已使用（消耗型装备进入销毁区）
        public void MarkEquipmentAsUsed(EquipmentTypeId equipmentTypeId)
        {
            if (equipmentTypeId != null)
            {
                EquipmentInventory.Instance.MarkAsDestroyed(equipmentTypeId);
                EquipmentInventory.Instance.MarkAsUndeployed(equipmentTypeId);
                
                onEquipmentMarkedAsUsed?.Invoke(equipmentTypeId);
            }
        }

        // 检查装备是否已被使用（检查销毁区数量是否大于等于总数量）
        public bool IsEquipmentUsed(EquipmentTypeId equipmentTypeId)
        {
            if (equipmentTypeId == null) return false;
            var totalCount =
                EquipmentInventory.Instance.Equipments.Count(e => e.TypeId.Equals(equipmentTypeId) && e.IsConsumable);
            var destroyedCount = EquipmentInventory.Instance.DestroyedEquipments.GetValueOrDefault(equipmentTypeId, 0);
            return destroyedCount >= totalCount;
        }

        // 获取背包中指定类型装备的数量
        private int GetEquipmentCountInInventory(EquipmentTypeId equipmentTypeId)
        {
            if (EquipmentInventory.Instance == null) return 0;

            return EquipmentInventory.Instance.Equipments
                .Count(equipment => equipment.TypeId.Equals(equipmentTypeId) && equipment.IsConsumable);
        }

        // 标记装备为已刷新（未刷新区->已刷新区）
        public void MarkEquipmentAsSpawned(EquipmentTypeId equipmentTypeId)
        {
            if (equipmentTypeId != null)
            {
                EquipmentInventory.Instance.MarkAsRefreshed(equipmentTypeId);
                Debug.Log($"标记装备为已刷新: {equipmentTypeId.Id}");
                onEquipmentMarkedAsSpawned?.Invoke(equipmentTypeId);
            }
        }

        // 检查装备是否已被刷新（检查已刷新区数量是否大于等于总数量）
        public bool IsEquipmentSpawned(EquipmentTypeId equipmentTypeId)
        {
            if (equipmentTypeId == null) return false;
            var inv = EquipmentInventory.Instance;
            // 只用 R 判断是否“已被抽选且未重洗”，避免在场数量阻塞下一回合再抽
            var refreshed = inv.GetRefreshedCount(equipmentTypeId);
            return refreshed > 0;
        }

        // 获取背包中指定类型装备的总数量（包括消耗型和非消耗型）
        private int GetTotalEquipmentCountInInventory(EquipmentTypeId equipmentTypeId)
        {
            if (EquipmentInventory.Instance == null) return 0;

            return EquipmentInventory.Instance.Equipments
                .Count(equipment => equipment.TypeId.Equals(equipmentTypeId));
        }

        // 重置弃牌区（将已刷新区的装备洗入未刷新区）
        public void ResetSpawnedEquipments()
        {
            EquipmentInventory.Instance.ResetRefreshed();
            Debug.Log("重置了已刷新区（弃牌区洗入抽牌区）");
            onSpawnedEquipmentsReset?.Invoke();
        }

        // 重置所有消耗型装备状态（销毁区洗入未刷新区）
        public void ResetAllConsumableEquipments()
        {
            EquipmentInventory.Instance.ResetDestroyed();
            Debug.Log("重置了所有消耗型装备的使用状态");
            onAllConsumableEquipmentsReset?.Invoke();
        }

        // 重置所有状态（新关卡开始时调用）
        public void ResetAllStates()
        {
            // 先清空在场区计数，避免上一关残留的在场计数影响 R→U 搬运
            EquipmentInventory.Instance.ResetInPlayCounts();
            Debug.Log("[EquipSpawn] 清空在场区计数");
            EquipmentInventory.Instance.ResetAllZones();
            Debug.Log("重置了所有装备状态");
            onAllStatesReset?.Invoke();
        }

        // 根据抽牌机制选择要刷新的装备
        public List<T> SelectEquipmentsToSpawn<T>(List<T> availableEquipments, int maxCount) where T : class
        {
            if (availableEquipments == null || availableEquipments.Count == 0)
                return new List<T>();

            // 使用抽牌机制选择装备
            return SelectEquipmentsWithDrawMechanism(availableEquipments, maxCount);
        }

        // 使用抽牌机制选择装备
        private List<T> SelectEquipmentsWithDrawMechanism<T>(List<T> availableEquipments, int maxCount) where T : class
        {
            var selectedEquipments = new List<T>();
            var remainingEquipments = new List<T>(availableEquipments);
            var rand = new Random();

            // 记录本轮抽选中已选中的装备类型及数量（避免同一轮重复抽选）
            var currentRoundSelectedCounts = new Dictionary<EquipmentTypeId, int>();

            while (selectedEquipments.Count < maxCount && remainingEquipments.Count > 0)
            {
                // 分离抽牌区（U可用）和弃牌区（非U可用）
                var drawPile = remainingEquipments
                    .Where(eq => CanEquipmentBeDrawnInCurrentRound(eq, currentRoundSelectedCounts)).ToList();
                var discardPile = remainingEquipments
                    .Where(eq => !CanEquipmentBeDrawnInCurrentRound(eq, currentRoundSelectedCounts)).ToList();
                

                // 如果抽牌区为空：按需触发部分重置（R→U），仅搬运未在场数量
                if (drawPile.Count == 0 && discardPile.Count > 0)
                {
                    // 只重置本轮未选中的类型，保护已选；并且仅搬 (R-P)
                    
                    ResetSpawnedEquipmentsExceptCurrentRound(currentRoundSelectedCounts);

                    // 重新计算抽牌区（现在 U 可能增加）
                    drawPile = remainingEquipments
                        .Where(eq => CanEquipmentBeDrawnInCurrentRound(eq, currentRoundSelectedCounts)).ToList();

                    
                }

                // 如果还是没有可选装备，退出循环
                if (drawPile.Count == 0)
                {
                    
                    break;
                }

                // 从抽牌区随机选择一个装备
                var randomIndex = rand.Next(drawPile.Count);
                var selectedEquipment = drawPile[randomIndex];

                selectedEquipments.Add(selectedEquipment);
                remainingEquipments.Remove(selectedEquipment);

                // 标记为已刷新并记录到本轮计数
                var equipmentTypeId = GetEquipmentTypeId(selectedEquipment);
                if (equipmentTypeId != null)
                {
                    
                    MarkEquipmentAsSpawned(equipmentTypeId);
                    

                    // 更新本轮选中计数
                    currentRoundSelectedCounts[equipmentTypeId] =
                        currentRoundSelectedCounts.GetValueOrDefault(equipmentTypeId, 0) + 1;
                }
            }

            
            return selectedEquipments;
        }

        // 判断装备是否可以在当前轮被抽选
        private bool CanEquipmentBeDrawnInCurrentRound<T>(T equipment,
            Dictionary<EquipmentTypeId, int> currentRoundSelectedCounts) where T : class
        {
            var equipmentTypeId = GetEquipmentTypeId(equipment);
            if (equipmentTypeId == null) return false;

            // 如果是消耗型装备且已被使用，则不能抽选
            if (equipment is EquipmentBase equipBase && equipBase.IsConsumable)
                if (IsEquipmentUsed(equipmentTypeId))
                {
                    Debug.Log($"[EquipSpawn] Skip {equipmentTypeId.Id} reason=consumable used");
                    return false;
                }

            // 允许同类型在同一回合被多次抽取，只要未刷新区仍有可用数量
            var inv = EquipmentInventory.Instance;
            var unrefreshed = inv != null ? inv.GetUnrefreshedCount(equipmentTypeId) : 0;
            var alreadySelected = currentRoundSelectedCounts.GetValueOrDefault(equipmentTypeId, 0);
            if (unrefreshed - alreadySelected <= 0) return false;

            // 检查本轮是否已经选中了足够数量的该类型装备
            var currentRoundSelected = currentRoundSelectedCounts.GetValueOrDefault(equipmentTypeId, 0);
            var totalAvailableOfThisType = GetTotalAvailableCountOfType(equipmentTypeId);

            // 如果本轮已选中的数量达到了该类型的总可用数量，则不能再抽选
            return currentRoundSelected < totalAvailableOfThisType;
        }

        // 获取指定类型装备的总可用数量（排除已使用的消耗型装备）
        private int GetTotalAvailableCountOfType(EquipmentTypeId equipmentTypeId)
        {
            if (EquipmentInventory.Instance == null) return 0;

            var totalCount = EquipmentInventory.Instance.Equipments
                .Count(equipment => equipment.TypeId.Equals(equipmentTypeId));

            // 如果是消耗型装备，需要减去已使用的数量
            var usedCount = GetUsedConsumableEquipmentCount(equipmentTypeId);

            return totalCount - usedCount;
        }

        // 重置弃牌区装备状态（排除当前轮已选中的装备）
        private void ResetSpawnedEquipmentsExceptCurrentRound(
            Dictionary<EquipmentTypeId, int> currentRoundSelectedCounts)
        {
            if (currentRoundSelectedCounts == null || currentRoundSelectedCounts.Count == 0)
            {
                // 如果本轮没有选中任何装备，使用完全重置
                ResetSpawnedEquipments();
                return;
            }

            var resetCount = 0;
            var inv = EquipmentInventory.Instance;
            var typesToProcess = inv.RefreshedEquipments.Keys.ToList();
            foreach (var typeId in typesToProcess)
            {
                var currentSpawnedCount = inv.GetRefreshedCount(typeId);
                var inPlayCount = inv.GetInPlayCount(typeId);
                var currentRoundSelected = currentRoundSelectedCounts.GetValueOrDefault(typeId, 0);
                if (currentRoundSelected > 0)
                {
                    // 保护本轮已选与在场数量，仅回收 R - P - selected
                    var resetAmount = Mathf.Max(0, currentSpawnedCount - inPlayCount - currentRoundSelected);
                    if (resetAmount > 0)
                    {
                        // 将多余部分转回未刷新区
                        inv.MoveRefreshedToUnrefreshed(typeId, resetAmount);
                        resetCount += resetAmount;
                    }
                }
                else
                {
                    // 本轮未选中该类型，完全重置
                    // MoveRefreshedToUnrefreshed 内部已按 (R - P) 限制搬运
                    resetCount += currentSpawnedCount;
                    inv.MoveRefreshedToUnrefreshed(typeId, currentSpawnedCount);
                }
            }

            Debug.Log($"部分重置了 {resetCount} 个装备的刷新状态（保护在场与本轮已选中的 {currentRoundSelectedCounts.Values.Sum()} 个装备）");
            if (resetCount > 0) onSpawnedEquipmentsReset?.Invoke();
        }

        // 获取装备的TypeId（需要根据具体装备类型实现）
        private EquipmentTypeId GetEquipmentTypeId<T>(T equipment) where T : class
        {
            // 尝试从装备对象获取TypeId
            if (equipment is EquipmentBase eq) return eq.TypeId;

            // 如果有其他装备类型，在这里添加相应的处理逻辑
            return null;
        }

        // 获取已使用的消耗型装备总数量
        public int GetUsedConsumableEquipmentCount()
        {
            return EquipmentInventory.Instance.DestroyedEquipments.Values.Sum();
        }

        // 获取指定类型消耗型装备的使用数量
        public int GetUsedConsumableEquipmentCount(EquipmentTypeId equipmentTypeId)
        {
            return equipmentTypeId != null
                ? EquipmentInventory.Instance.DestroyedEquipments.GetValueOrDefault(equipmentTypeId, 0)
                : 0;
        }

        // 获取已刷新的装备总数量
        public int GetSpawnedEquipmentCount()
        {
            return EquipmentInventory.Instance.RefreshedEquipments.Values.Sum();
        }

        // 获取指定类型装备的刷新数量
        public int GetSpawnedEquipmentCount(EquipmentTypeId equipmentTypeId)
        {
            return equipmentTypeId != null
                ? EquipmentInventory.Instance.RefreshedEquipments.GetValueOrDefault(equipmentTypeId, 0)
                : 0;
        }

        // 获取所有已使用的消耗型装备类型及其数量（只读）
        public IReadOnlyDictionary<EquipmentTypeId, int> GetUsedConsumableEquipmentCounts()
        {
            return EquipmentInventory.Instance.DestroyedEquipments;
        }

        // 获取所有已刷新的装备类型及其数量（只读）
        public IReadOnlyDictionary<EquipmentTypeId, int> GetSpawnedEquipmentCounts()
        {
            return EquipmentInventory.Instance.RefreshedEquipments;
        }

        // 随机化位置列表
        private List<Vector2Int> ShufflePositions(List<Vector2Int> positions)
        {
            if (positions == null || positions.Count == 0)
                return new List<Vector2Int>();

            var shuffled = new List<Vector2Int>(positions);
            var rand = new Random();
            
            // 使用Fisher-Yates洗牌算法
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
            }
            
            return shuffled;
        }
    }
}