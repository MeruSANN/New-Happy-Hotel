using System;
using System.Linq;
using HappyHotel.Core.Grid;
using HappyHotel.Core.Singleton;
using HappyHotel.Device;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace HappyHotel.Map
{
    [ManagedSingleton(SceneLoadMode.Exclude, "ShopScene", "MainMenu")]
    public class MapManager : SingletonBase<MapManager>
    {
        private const string TILES_PATH_PREFIX = "Tiles/";
        private readonly string floorTileName = "Floor Wood";
        private readonly string wallTileName = "Wall";
        private TileBase floorTile;

        private Grid grid;

        // 存储地图逻辑数据
        private TileInfo[,] mapData;
        private int mapHeight = 15;
        private Tilemap mapTilemap;

        private int mapWidth = 20;
        private TileBase wallTile;

        private void Start()
        {
            // 查找场景中的Grid组件
            grid = GameObject.FindWithTag("Grid")?.GetComponent<Grid>();
            if (!grid)
            {
                Debug.LogError("找不到Grid!");
                return;
            }

            mapTilemap = GameObject.FindWithTag("MapTilemap")?.GetComponent<Tilemap>();

            if (!mapTilemap)
            {
                Debug.LogWarning("找不到Map Tilemap!");
                return;
            }

            // 将逻辑地图映射到视觉Tilemap上
            UpdateVisualMap();

            // 触发初始地图大小事件，确保其他组件能够正确初始化
            onMapSizeChanged?.Invoke(new Vector2Int(mapWidth, mapHeight));
        }

        // 地图大小改变事件
        public event Action<Vector2Int> onMapSizeChanged;

        public void SetSize(int mapWidth, int mapHeight)
        {
            var sizeChanged = this.mapWidth != mapWidth || this.mapHeight != mapHeight;

            this.mapWidth = mapWidth;
            this.mapHeight = mapHeight;

            // 重新初始化地图数据
            InitializeMap();

            // 如果大小发生了变化，触发事件
            if (sizeChanged) onMapSizeChanged?.Invoke(new Vector2Int(mapWidth, mapHeight));
        }

        public void SetWallTile(TileBase tile)
        {
            wallTile = tile;
        }

        public void SetFloorTile(TileBase tile)
        {
            floorTile = tile;
        }

        protected override void OnSingletonAwake()
        {
            LoadTilesFromFile();
            InitializeMap();
        }

        private void LoadTilesFromFile()
        {
            var wallTilePath = TILES_PATH_PREFIX + wallTileName;
            var floorTilePath = TILES_PATH_PREFIX + floorTileName;

            // 从Resources/Tiles文件夹加载tile资源
            wallTile = Resources.Load<TileBase>(wallTilePath);
            floorTile = Resources.Load<TileBase>(floorTilePath);

            if (wallTile == null) Debug.LogError($"无法加载wallTile资源: Resources/{wallTilePath}");

            if (floorTile == null) Debug.LogError($"无法加载floorTile资源: Resources/{floorTilePath}");
        }

        private void InitializeMap()
        {
            // 创建地图数组
            mapData = new TileInfo[mapWidth, mapHeight];

            // 先将所有地格初始化为空地
            for (var x = 0; x < mapWidth; x++)
            for (var y = 0; y < mapHeight; y++)
                mapData[x, y] = new TileInfo(TileType.Empty);
        }

        public void UpdateVisualMap()
        {
            if (!mapTilemap) return;

            // 清除所有Tilemap
            mapTilemap.ClearAllTiles();

            // 遍历地图数据，更新对应的Tilemap
            for (var x = 0; x < mapWidth; x++)
            for (var y = 0; y < mapHeight; y++)
            {
                var tileInfo = GetTile(x, y);
                var cellPos = new Vector3Int(x, y, 0);

                // 根据地格类型，在对应的Tilemap上放置特定的Tile
                switch (tileInfo.Type)
                {
                    case TileType.Wall:
                        mapTilemap.SetTile(cellPos, wallTile);
                        break;

                    case TileType.Floor:
                        mapTilemap.SetTile(cellPos, floorTile);
                        break;

                    case TileType.Empty:
                        // 空地不渲染任何贴图
                        mapTilemap.SetTile(cellPos, null);
                        break;
                }
            }
        }

        // 获取指定位置的地格信息
        public TileInfo GetTile(int x, int y)
        {
            // 检查边界
            if (x >= 0 && x < mapWidth && y >= 0 && y < mapHeight) return mapData[x, y];

            // 边界外默认为墙体
            return new TileInfo(TileType.Wall);
        }

        // 设置指定位置的地格信息
        public void SetTile(int x, int y, TileInfo tileInfo)
        {
            // 检查边界
            if (x >= 0 && x < mapWidth && y >= 0 && y < mapHeight) mapData[x, y] = tileInfo;
        }

        public void SetTile(Vector2Int position, TileInfo tileInfo)
        {
            SetTile(position.x, position.y, tileInfo);
        }

        // 获取地图尺寸
        public Vector2Int GetMapSize()
        {
            return new Vector2Int(mapWidth, mapHeight);
        }

        // 检查位置是否有阻挡性Device
        private bool HasBlockingDeviceAt(int x, int y)
        {
            if (GridObjectManager.Instance == null) return false;

            var devices = GridObjectManager.Instance.GetObjectsOfTypeAt<DeviceBase>(new Vector2Int(x, y));
            return devices.Any(device => device is IBlockingDevice);
        }

        // 检查指定位置是否可以通行
        public bool IsWalkable(int x, int y)
        {
            return !IsWall(x, y);
        }

        public bool IsFloor(int x, int y)
        {
            var tile = GetTile(x, y);
            return tile.Type == TileType.Floor;
        }

        // 检查指定位置是否是墙体（包括阻挡性Device）
        public bool IsWall(int x, int y)
        {
            var tile = GetTile(x, y);
            var isWallTile = tile.Type == TileType.Wall;
            var hasBlockingDevice = HasBlockingDeviceAt(x, y);

            return isWallTile || hasBlockingDevice;
        }

        // 检查位置是否是角落（2面是墙且2面墙不在互相的对面方向）
        public bool IsCorner(int x, int y)
        {
            // 检查四个方向的墙体情况
            var upWall = IsWall(x, y + 1);
            var downWall = IsWall(x, y - 1);
            var leftWall = IsWall(x - 1, y);
            var rightWall = IsWall(x + 1, y);

            // 角落的定义：恰好有2面墙，且这2面墙不在对面方向
            var wallCount = (upWall ? 1 : 0) + (downWall ? 1 : 0) + (leftWall ? 1 : 0) + (rightWall ? 1 : 0);

            if (wallCount == 2)
            {
                // 检查是否是对面方向的墙（上下对面，左右对面）
                var isOppositeWalls = (upWall && downWall) || (leftWall && rightWall);
                return !isOppositeWalls; // 不是对面方向的墙才算角落
            }

            return false;
        }

        // 检查位置是否有3面及以上墙体
        public bool HasThreeOrMoreWalls(int x, int y)
        {
            // 检查四个方向的墙体情况
            var upWall = IsWall(x, y + 1);
            var downWall = IsWall(x, y - 1);
            var leftWall = IsWall(x - 1, y);
            var rightWall = IsWall(x + 1, y);

            var wallCount = (upWall ? 1 : 0) + (downWall ? 1 : 0) + (leftWall ? 1 : 0) + (rightWall ? 1 : 0);
            return wallCount >= 3;
        }

        // 检查位置是否应该被排除在Prop刷新之外（角落或三面墙）
        public bool ShouldExcludeFromPropSpawn(int x, int y)
        {
            return IsCorner(x, y) || HasThreeOrMoreWalls(x, y);
        }

        public Vector2Int GetMouseGridPosition()
        {
            // 获取鼠标在世界空间中的位置
            if (Camera.main)
            {
                var mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                // 将世界坐标转换为网格坐标
                var cellPosition = grid.WorldToCell(mouseWorldPos);

                return new Vector2Int(cellPosition.x, cellPosition.y);
            }

            return new Vector2Int(-1, -1);
        }
    }
}