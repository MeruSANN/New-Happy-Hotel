using System.Collections.Generic;
using HappyHotel.Character;
using HappyHotel.Device;
using HappyHotel.Enemy;
using HappyHotel.Map;
using HappyHotel.Prop;
using UnityEngine;
using Random = System.Random;

namespace HappyHotel.Core.Grid
{
    // 刷新位置验证器，提供公共的位置验证逻辑
    public static class SpawnPositionValidator
    {
        // 获取所有合法的刷新位置
        // 合法位置定义：为地面，上面没有其他GridObject，且不为角落或周围有3面以上的墙
        public static List<Vector2Int> GetValidSpawnPositions()
        {
            var mapManager = MapManager.Instance;
            var gridManager = GridObjectManager.Instance;

            if (!mapManager || !gridManager)
            {
                Debug.LogWarning("MapManager或GridObjectManager未初始化，无法获取合法刷新位置");
                return new List<Vector2Int>();
            }

            var mapSize = mapManager.GetMapSize();
            var validPositions = new List<Vector2Int>();

            for (var x = 0; x < mapSize.x; x++)
            for (var y = 0; y < mapSize.y; y++)
                if (IsValidSpawnPosition(x, y))
                    validPositions.Add(new Vector2Int(x, y));

            return validPositions;
        }

        // 检查指定位置是否为合法的刷新位置
        public static bool IsValidSpawnPosition(int x, int y)
        {
            return IsValidSpawnPosition(new Vector2Int(x, y));
        }

        // 检查指定位置是否为合法的刷新位置
        public static bool IsValidSpawnPosition(Vector2Int position)
        {
            var mapManager = MapManager.Instance;
            var gridManager = GridObjectManager.Instance;

            if (!mapManager || !gridManager) return false;

            // 必须是地板
            if (!mapManager.IsFloor(position.x, position.y)) return false;

            // 位置上不能有其他GridObject
            if (gridManager.GetObjectsAt(position).Count > 0) return false;

            // 不能是角落或三面墙的位置
            if (mapManager.ShouldExcludeFromPropSpawn(position.x, position.y)) return false;

            return true;
        }

        // 检查指定位置四周是否有其他对象（敌人、角色、Device、墙体）
        public static bool HasObjectsAround(Vector2Int position)
        {
            var gridManager = GridObjectManager.Instance;
            if (!gridManager) return false;

            // 检查四个方向
            var directions = new[]
            {
                Vector2Int.up,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right
            };

            foreach (var direction in directions)
            {
                var checkPosition = position + direction;
                var objectsAtPosition = gridManager.GetObjectsAt(checkPosition);
                
                // 检查是否有敌人、角色、Device
                foreach (var obj in objectsAtPosition)
                {
                    if (obj is EnemyBase || obj is CharacterBase || obj is DeviceBase)
                    {
                        return true;
                    }
                }

                // 检查是否是墙体
                var mapManager = MapManager.Instance;
                if (mapManager != null && mapManager.IsWall(checkPosition.x, checkPosition.y))
                {
                    return true;
                }
            }

            return false;
        }

        // 获取装备刷新的合法位置（四周都没有其他对象）
        public static List<Vector2Int> GetValidEquipmentSpawnPositions()
        {
            var mapManager = MapManager.Instance;
            var gridManager = GridObjectManager.Instance;

            if (!mapManager || !gridManager)
            {
                Debug.LogWarning("MapManager或GridObjectManager未初始化，无法获取合法刷新位置");
                return new List<Vector2Int>();
            }

            var mapSize = mapManager.GetMapSize();
            var validPositions = new List<Vector2Int>();

            for (var x = 0; x < mapSize.x; x++)
            for (var y = 0; y < mapSize.y; y++)
            {
                var position = new Vector2Int(x, y);
                if (IsValidSpawnPosition(position) && !HasObjectsAround(position))
                {
                    validPositions.Add(position);
                }
            }

            return validPositions;
        }

        // 从合法位置列表中随机选择指定数量的位置
        public static List<Vector2Int> SelectRandomPositions(List<Vector2Int> validPositions, int count)
        {
            if (validPositions == null || validPositions.Count == 0) return new List<Vector2Int>();

            if (count >= validPositions.Count) return new List<Vector2Int>(validPositions);

            var rand = new Random();
            var shuffledPositions = new List<Vector2Int>(validPositions);

            // Fisher-Yates洗牌算法
            for (var i = shuffledPositions.Count - 1; i > 0; i--)
            {
                var randomIndex = rand.Next(i + 1);
                var temp = shuffledPositions[i];
                shuffledPositions[i] = shuffledPositions[randomIndex];
                shuffledPositions[randomIndex] = temp;
            }

            return shuffledPositions.GetRange(0, count);
        }

        // 获取指定位置周围的合法刷新位置（用于在特定区域刷新）
        public static List<Vector2Int> GetValidSpawnPositionsAround(Vector2Int center, int radius)
        {
            var validPositions = new List<Vector2Int>();

            for (var x = center.x - radius; x <= center.x + radius; x++)
            for (var y = center.y - radius; y <= center.y + radius; y++)
            {
                var pos = new Vector2Int(x, y);
                if (IsValidSpawnPosition(pos)) validPositions.Add(pos);
            }

            return validPositions;
        }
    }
}