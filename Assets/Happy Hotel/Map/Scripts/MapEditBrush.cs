using System;
using System.Linq;
using HappyHotel.Character;
using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.Grid;
using HappyHotel.Core.Grid.Components;
using HappyHotel.Core.Registry;
using HappyHotel.Core.Singleton;
using HappyHotel.Device;
using HappyHotel.Enemy;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using CharacterController = HappyHotel.Character.CharacterController;

namespace HappyHotel.Map
{
    // 笔刷模式枚举
    public enum BrushMode
    {
        DrawTile, // 绘制砖块模式
        PlaceObject, // 放置对象模式
        PlaceMainCharacter, // 放置主角色模式
        DeleteObject // 删除对象模式
    }

    // 对象类型枚举
    public enum ObjectType
    {
        Device, // 装置类型
        Enemy // 敌人类型
    }

    // 运行时地图编辑笔刷
    public class MapEditBrush : SingletonBase<MapEditBrush>
    {
        [Header("笔刷设置")] [SerializeField] private BrushMode currentMode = BrushMode.DrawTile;

        [Header("UI交互设置")] [SerializeField] private bool showDebugInfo; // 是否显示调试信息

        // GridObject放置相关
        private ObjectType currentObjectType = ObjectType.Device;
        private string currentObjectTypeString = "";

        // Tile绘制相关
        private TileType currentTileType = TileType.Floor;

        // 缓存的组件引用
        private Grid grid;

        // 连续绘制相关
        private bool isMouseDown;
        private Vector2Int lastDrawPosition = Vector2Int.one * -1; // 上次绘制的位置
        private Tilemap mapTilemap;

        private void Update()
        {
            HandleMouseInput();
        }

        protected override void OnSingletonAwake()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // 获取Grid组件
            grid = GameObject.FindWithTag("Grid")?.GetComponent<Grid>();
            if (!grid)
            {
                Debug.LogError("MapEditBrush: 找不到Grid组件!");
                return;
            }

            // 获取MapTilemap组件
            mapTilemap = GameObject.FindWithTag("MapTilemap")?.GetComponent<Tilemap>();
            if (!mapTilemap) Debug.LogError("MapEditBrush: 找不到MapTilemap组件!");
        }

        private void HandleMouseInput()
        {
            var isOverUI = IsPointerOverUI();

            // 在删除模式下，允许点击UI元素（如敌人）
            if (isOverUI && currentMode != BrushMode.DeleteObject) return;

            // 鼠标按下
            if (Input.GetMouseButtonDown(0))
            {
                isMouseDown = true;
                lastDrawPosition = Vector2Int.one * -1; // 重置上次绘制位置
                HandleMouseAction();
            }
            // 鼠标抬起
            else if (Input.GetMouseButtonUp(0))
            {
                isMouseDown = false;
                lastDrawPosition = Vector2Int.one * -1; // 重置上次绘制位置
            }
            // 鼠标按住拖拽（仅在Tile模式下连续绘制）
            else if (isMouseDown && currentMode == BrushMode.DrawTile)
            {
                HandleMouseAction();
            }
        }

        // 检查鼠标指针是否在UI元素上
        private bool IsPointerOverUI()
        {
            // 检查是否有EventSystem
            if (EventSystem.current == null) return false;

            // 使用EventSystem检查鼠标是否在UI上
            return EventSystem.current.IsPointerOverGameObject();
        }

        private void HandleMouseAction()
        {
            var gridPosition = GetMouseGridPosition();

            // 在连续绘制模式下，如果位置没有改变，则跳过
            if (currentMode == BrushMode.DrawTile && lastDrawPosition == gridPosition) return;

            switch (currentMode)
            {
                case BrushMode.DrawTile:
                    DrawTile(gridPosition);
                    lastDrawPosition = gridPosition; // 记录绘制位置
                    break;

                case BrushMode.PlaceObject:
                    // 对象放置模式只在鼠标按下时执行，不支持连续放置
                    if (Input.GetMouseButtonDown(0)) PlaceGridObject(gridPosition);
                    break;

                case BrushMode.PlaceMainCharacter:
                    // 主角色放置模式只在鼠标按下时执行，不支持连续放置
                    if (Input.GetMouseButtonDown(0)) PlaceMainCharacter(gridPosition);
                    break;

                case BrushMode.DeleteObject:
                    // 删除对象模式只在鼠标按下时执行，不支持连续删除
                    if (Input.GetMouseButtonDown(0)) DeleteObjectAt(gridPosition);
                    break;
            }
        }

        private Vector2Int GetMouseGridPosition()
        {
            if (!grid || !Camera.main) return Vector2Int.zero;

            var mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var cellPosition = grid.WorldToCell(mouseWorldPos);
            return new Vector2Int(cellPosition.x, cellPosition.y);
        }

        private void DrawTile(Vector2Int position)
        {
            if (!mapTilemap || !MapManager.Instance)
            {
                Debug.LogWarning("MapEditBrush: MapTilemap或MapManager未初始化");
                return;
            }

            // 检查是否超出地图边界
            if (!IsPositionInMapBounds(position)) return;

            // 在逻辑地图中设置砖块类型
            var tileInfo = new TileInfo(currentTileType);
            MapManager.Instance.SetTile(position, tileInfo);

            // 在视觉Tilemap中设置砖块
            var cellPos = new Vector3Int(position.x, position.y, 0);

            switch (currentTileType)
            {
                case TileType.Empty:
                    mapTilemap.SetTile(cellPos, null);
                    break;
                default:
                    // 使用MapManager的默认Tile
                    MapManager.Instance.UpdateVisualMap();
                    break;
            }
        }

        private void PlaceGridObject(Vector2Int position)
        {
            if (string.IsNullOrEmpty(currentObjectTypeString))
            {
                Debug.LogWarning("MapEditBrush: 未设置要放置的对象类型");
                return;
            }

            // 检查是否超出地图边界
            if (!IsPositionInMapBounds(position))
            {
                Debug.LogWarning($"MapEditBrush: 位置 {position} 超出地图边界，无法放置对象");
                return;
            }

            if (!CanPlaceObjectAt(position)) return;

            // 根据对象类型使用对应的Manager创建对象
            switch (currentObjectType)
            {
                case ObjectType.Device:
                    PlaceDevice(position);
                    break;

                case ObjectType.Enemy:
                    PlaceEnemy(position);
                    break;
            }
        }

        private bool PlaceDevice(Vector2Int position)
        {
            if (!DeviceManager.Instance)
            {
                Debug.LogError("MapEditBrush: DeviceManager未初始化");
                return false;
            }

            try
            {
                var deviceTypeId = TypeId.Create<DeviceTypeId>(currentObjectTypeString);
                var device = DeviceController.Instance.PlaceDevice(position, deviceTypeId);
                return device != null;
            }
            catch (Exception e)
            {
                Debug.LogError($"MapEditBrush: 创建装置失败 - {e.Message}");
                return false;
            }
        }

        private bool PlaceEnemy(Vector2Int position)
        {
            if (!EnemyManager.Instance)
            {
                Debug.LogError("MapEditBrush: EnemyManager未初始化");
                return false;
            }

            try
            {
                var enemyTypeId = TypeId.Create<EnemyTypeId>(currentObjectTypeString);
                var enemy = EnemyController.Instance.CreateEnemy(enemyTypeId, position);
                return enemy != null;
            }
            catch (Exception e)
            {
                Debug.LogError($"MapEditBrush: 创建敌人失败 - {e.Message}");
                return false;
            }
        }

        // 放置主角色的特殊方法
        public bool PlaceMainCharacter(Vector2Int position)
        {
            // 检查是否超出地图边界
            if (!IsPositionInMapBounds(position))
            {
                Debug.LogWarning($"MapEditBrush: 位置 {position} 超出地图边界，无法放置主角色");
                return false;
            }

            // 检查目标位置砖块类型是否为空
            if (MapManager.Instance)
            {
                var tileInfo = MapManager.Instance.GetTile(position.x, position.y);
                if (tileInfo.Type == TileType.Empty) return false;
            }

            // 查找现有的MainCharacter
            var existingMainCharacter = GameObject.FindGameObjectWithTag("MainCharacter");

            if (existingMainCharacter != null)
            {
                // 如果已存在MainCharacter，将其移动到新位置
                var gridObjectComponent = existingMainCharacter.GetComponent<BehaviorComponentContainer>()
                    .GetBehaviorComponent<GridObjectComponent>();
                gridObjectComponent.MoveTo(position);
                return true;
            }

            // 如果不存在MainCharacter，创建一个新的DefaultCharacter
            if (!CharacterManager.Instance)
            {
                Debug.LogError("MapEditBrush: CharacterManager未初始化");
                return false;
            }

            try
            {
                var defaultCharacterTypeId = TypeId.Create<CharacterTypeId>("Default");
                var character = CharacterController.Instance.CreateCharacter(defaultCharacterTypeId, position);

                if (character != null)
                {
                    // 设置为MainCharacter标签
                    character.tag = "MainCharacter";
                    return true;
                }

                Debug.LogError("创建DefaultCharacter失败");
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"MapEditBrush: 创建MainCharacter失败 - {e.Message}");
                return false;
            }
        }

        // 删除指定位置的对象
        private void DeleteObjectAt(Vector2Int position)
        {
            // 检查是否超出地图边界
            if (!IsPositionInMapBounds(position))
            {
                Debug.LogWarning($"MapEditBrush: 位置 {position} 超出地图边界，无法删除对象");
                return;
            }

            // 删除该位置的所有GridObject
            if (GridObjectManager.Instance)
            {
                var objectsAtPosition = GridObjectManager.Instance.GetObjectsAt(position);

                if (objectsAtPosition.Count > 0)
                    foreach (var gridObject in objectsAtPosition.ToList()) // 使用ToList避免修改集合时的问题
                    {
                        var gameObject = gridObject.gameObject;

                        // 销毁GameObject
                        DestroyImmediate(gameObject);
                    }
            }
        }

        private bool CanPlaceObjectAt(Vector2Int position)
        {
            // 检查该位置是否已有其他GridObject
            if (GridObjectManager.Instance)
            {
                var objectsAtPosition = GridObjectManager.Instance.GetObjectsAt(position);
                if (objectsAtPosition.Count > 0) return false;
            }

            // 检查目标位置砖块类型是否为空
            if (MapManager.Instance)
            {
                var tileInfo = MapManager.Instance.GetTile(position.x, position.y);
                if (tileInfo.Type == TileType.Empty) return false;
            }

            return true;
        }

        // 检查位置是否在地图边界内
        private bool IsPositionInMapBounds(Vector2Int position)
        {
            if (!MapManager.Instance)
            {
                Debug.LogWarning("MapEditBrush: MapManager未初始化，无法检查地图边界");
                return false;
            }

            var mapSize = MapManager.Instance.GetMapSize();

            // 检查X和Y坐标是否在有效范围内
            return position.x >= 0 && position.x < mapSize.x &&
                   position.y >= 0 && position.y < mapSize.y;
        }

        // ===== 公共API：设置笔刷内容 =====

        // 设置笔刷模式
        public void SetBrushMode(BrushMode mode)
        {
            currentMode = mode;
            Debug.Log($"笔刷模式设置为: {mode}");
        }

        // 设置要绘制的砖块类型
        public void SetTileType(TileType tileType)
        {
            currentTileType = tileType;
            Debug.Log($"设置砖块类型为: {tileType}");
        }

        // 设置要放置的装置类型
        public void SetDeviceType(string deviceTypeString)
        {
            currentObjectType = ObjectType.Device;
            currentObjectTypeString = deviceTypeString;
            Debug.Log($"设置装置类型为: {deviceTypeString}");
        }

        // 设置要放置的敌人类型
        public void SetEnemyType(string enemyTypeString)
        {
            currentObjectType = ObjectType.Enemy;
            currentObjectTypeString = enemyTypeString;
            Debug.Log($"设置敌人类型为: {enemyTypeString}");
        }

        // 设置笔刷为主角色放置模式
        public void SetMainCharacterMode()
        {
            currentMode = BrushMode.PlaceMainCharacter;
            Debug.Log("笔刷模式设置为: 放置主角色模式");
        }

        // 设置笔刷为删除模式
        public void SetDeleteMode()
        {
            currentMode = BrushMode.DeleteObject;
            Debug.Log("笔刷模式设置为: 删除对象模式");
        }

        // ===== UI交互控制API =====

        // 设置是否显示调试信息
        public void SetShowDebugInfo(bool show)
        {
            showDebugInfo = show;
            Debug.Log($"设置调试信息显示为: {show}");
        }


        // 手动检查当前鼠标是否在UI上
        public bool IsCurrentlyOverUI()
        {
            return IsPointerOverUI();
        }

        // ===== 获取当前设置的API =====

        public BrushMode GetCurrentMode()
        {
            return currentMode;
        }

        public TileType GetCurrentTileType()
        {
            return currentTileType;
        }

        public ObjectType GetCurrentObjectType()
        {
            return currentObjectType;
        }

        public string GetCurrentObjectTypeString()
        {
            return currentObjectTypeString;
        }
    }
}