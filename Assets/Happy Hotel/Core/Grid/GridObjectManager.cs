using System;
using System.Collections.Generic;
using System.Linq;
using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.Grid.Components;
using HappyHotel.Core.Singleton;
using HappyHotel.Map;
using UnityEngine;

namespace HappyHotel.Core.Grid
{
    // 负责管理所有网格对象的位置和交互
    [ManagedSingleton(SceneLoadMode.Exclude, "ShopScene", "MainMenu")]
    public class GridObjectManager : SingletonBase<GridObjectManager>
    {
        // 存储每个网格坐标上的对象列表
        private readonly Dictionary<Vector2Int, List<BehaviorComponentContainer>> gridObjects = new();

        // 为了快速查找特定类型的对象
        private readonly Dictionary<Type, List<BehaviorComponentContainer>> objectsByType = new();

        // 网格引用
        private UnityEngine.Grid grid;

        protected override void OnSingletonAwake()
        {
            var obj = GameObject.FindWithTag("Grid");
            if (!obj) Debug.LogError("GridObjectManager: 找不到Grid组件!");
            grid = obj.GetComponent<UnityEngine.Grid>();
        }

        // 获取GridObjectComponent
        private GridObjectComponent GetGridComponent(BehaviorComponentContainer container)
        {
            return container?.GetBehaviorComponent<GridObjectComponent>();
        }

        // 注册一个网格对象
        public void RegisterObject(BehaviorComponentContainer container, Vector2Int position)
        {
            var gridComponent = GetGridComponent(container);
            if (gridComponent == null)
            {
                Debug.LogError($"尝试注册没有GridObjectComponent的对象: {container.name}");
                return;
            }

            // 如果该位置没有对象列表，创建一个
            if (!gridObjects.ContainsKey(position)) gridObjects[position] = new List<BehaviorComponentContainer>();

            // 添加对象到位置列表
            gridObjects[position].Add(container);

            // 添加到类型字典中
            var objType = container.GetType();
            if (!objectsByType.ContainsKey(objType)) objectsByType[objType] = new List<BehaviorComponentContainer>();
            objectsByType[objType].Add(container);

            // 设置对象位置
            gridComponent.SetVisualGridPosition(position);

            // 触发该位置上所有其他对象的OnObjectEnter事件
            foreach (var existingObj in gridObjects[position].Where(x => x != container))
            {
                var existingGridComponent = GetGridComponent(existingObj);
                if (existingGridComponent != null)
                {
                    existingGridComponent.HandleObjectEnter(gridComponent);
                    gridComponent.HandleObjectEnter(existingGridComponent);
                }
            }

            Debug.Log($"已注册 {container.name} 到位置 {position}");
        }

        // 从网格中移除一个对象
        public void UnregisterObject(BehaviorComponentContainer container)
        {
            var gridComponent = GetGridComponent(container);
            if (gridComponent == null)
            {
                Debug.LogError($"尝试注销没有GridObjectComponent的对象: {container.name}");
                return;
            }

            var position = gridComponent.GetGridPosition();

            // 从位置字典中移除
            if (gridObjects.ContainsKey(position))
            {
                // 触发该位置上所有其他对象的OnObjectExit事件
                foreach (var existingObj in gridObjects[position].Where(x => x != container))
                {
                    var existingGridComponent = GetGridComponent(existingObj);
                    if (existingGridComponent != null)
                    {
                        existingGridComponent.HandleObjectExit(gridComponent);
                        gridComponent.HandleObjectExit(existingGridComponent);
                    }
                }

                gridObjects[position].Remove(container);

                // 如果列表为空，移除该位置的键
                if (gridObjects[position].Count == 0) gridObjects.Remove(position);
            }

            // 从类型字典中移除
            var objType = container.GetType();
            if (objectsByType.ContainsKey(objType))
            {
                objectsByType[objType].Remove(container);

                // 如果列表为空，移除该类型的键
                if (objectsByType[objType].Count == 0) objectsByType.Remove(objType);
            }
        }

        // 移动对象到新位置
        public bool MoveObject(BehaviorComponentContainer container, Vector2Int newPosition)
        {
            var gridComponent = GetGridComponent(container);
            if (gridComponent == null)
            {
                Debug.LogError($"尝试移动没有GridObjectComponent的对象: {container.name}");
                return false;
            }

            // 检查移动是否有效
            if (!IsValidMove(container, newPosition)) return false;

            var oldPosition = gridComponent.GetGridPosition();

            // 处理旧位置上对象的交互（离开事件）
            if (gridObjects.ContainsKey(oldPosition))
                foreach (var otherObj in gridObjects[oldPosition].Where(x => x != container))
                {
                    var otherGridComponent = GetGridComponent(otherObj);
                    if (otherGridComponent != null)
                    {
                        otherGridComponent.HandleObjectExit(gridComponent);
                        gridComponent.HandleObjectExit(otherGridComponent);
                    }
                }

            // 从旧位置移除
            if (gridObjects.ContainsKey(oldPosition))
            {
                gridObjects[oldPosition].Remove(container);

                // 如果列表为空，移除该位置的键
                if (gridObjects[oldPosition].Count == 0) gridObjects.Remove(oldPosition);
            }

            // 添加到新位置
            if (!gridObjects.ContainsKey(newPosition))
                gridObjects[newPosition] = new List<BehaviorComponentContainer>();
            gridObjects[newPosition].Add(container);

            // 设置对象的新位置
            gridComponent.SetVisualGridPosition(newPosition);

            // 处理新位置上对象的交互（进入事件）
            var canEnter = true;
            if (gridObjects.ContainsKey(newPosition))
                foreach (var otherObj in gridObjects[newPosition].Where(x => x != container))
                {
                    var otherGridComponent = GetGridComponent(otherObj);
                    if (otherGridComponent != null)
                        if (!otherGridComponent.HandleObjectEnter(gridComponent) ||
                            !gridComponent.HandleObjectEnter(otherGridComponent))
                        {
                            canEnter = false;
                            break;
                        }
                }

            Debug.Log($"已移动 {container.name} 从 {oldPosition} 到 {newPosition} ({Time.time})");
            return canEnter;
        }

        // 获取指定位置的所有对象
        public List<BehaviorComponentContainer> GetObjectsAt(Vector2Int position)
        {
            if (gridObjects.ContainsKey(position)) return new List<BehaviorComponentContainer>(gridObjects[position]);
            return new List<BehaviorComponentContainer>();
        }

        // 获取特定类型的对象
        public List<T> GetObjectsOfType<T>() where T : BehaviorComponentContainer
        {
            var type = typeof(T);
            if (objectsByType.TryGetValue(type, out var containers)) return containers.Cast<T>().ToList();

            // 如果没有直接匹配的类型，尝试找到实现了T接口或继承了T类的对象
            var result = new List<T>();
            foreach (var pair in objectsByType)
                if (typeof(T).IsAssignableFrom(pair.Key))
                    result.AddRange(pair.Value.Cast<T>());

            return result;
        }

        // 获取指定位置的特定类型对象
        public List<T> GetObjectsOfTypeAt<T>(Vector2Int position) where T : BehaviorComponentContainer
        {
            if (!gridObjects.ContainsKey(position)) return new List<T>();

            return gridObjects[position]
                .Where(obj => obj is T)
                .Cast<T>()
                .ToList();
        }

        // 世界坐标转网格坐标
        public Vector2Int WorldToGrid(Vector3 worldPosition)
        {
            if (grid == null)
            {
                Debug.LogError("GridObjectManager: Grid组件未分配!");
                return Vector2Int.zero;
            }

            var cellPosition = grid.WorldToCell(worldPosition);
            return new Vector2Int(cellPosition.x, cellPosition.y);
        }

        // 网格坐标转世界坐标
        public Vector3 GridToWorld(Vector2Int gridPosition)
        {
            if (!grid)
            {
                Debug.LogError("GridObjectManager: Grid组件未分配!");
                return Vector3.zero;
            }

            var cellPosition = new Vector3Int(gridPosition.x, gridPosition.y, 0);
            return grid.GetCellCenterWorld(cellPosition);
        }

        // 检查位置是否有阻挡物体
        public bool IsPositionBlocked(Vector2Int position)
        {
            // 首先使用MapManager检查是否是墙体
            if (MapManager.Instance != null)
                if (!MapManager.Instance.IsWalkable(position.x, position.y))
                    // 位置是墙体，不能通行
                    return true;

            return false;
        }

        // 检查移动是否有效（碰撞检测等）
        public bool IsValidMove(BehaviorComponentContainer container, Vector2Int newPosition)
        {
            var gridComponent = GetGridComponent(container);

            // 如果对象可以放在墙体上，跳过墙体检查
            if (gridComponent != null && gridComponent.CanPlaceOnWalls()) return true;

            // 检查位置是否被阻挡
            if (IsPositionBlocked(newPosition)) return false;

            return true;
        }

        public void ClearAllObjects()
        {
            objectsByType.Clear();

            foreach (var obj in gridObjects)
            {
                var containers = obj.Value;
                foreach (var container in containers) Destroy(container.gameObject);
            }

            gridObjects.Clear();
        }
    }
}