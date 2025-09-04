using System;
using System.Collections.Generic;
using System.IO;
using HappyHotel.Character;
using HappyHotel.Core;
using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.Grid;
using HappyHotel.Core.Grid.Components;
using HappyHotel.Core.Registry;
using HappyHotel.Core.Singleton;
using HappyHotel.Device;
using HappyHotel.Enemy;
using HappyHotel.GameManager;
using HappyHotel.Map.Data;
using UnityEngine;
using CharacterController = HappyHotel.Character.CharacterController;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HappyHotel.Map
{
    [ManagedSingleton(SceneLoadMode.Exclude, "ShopScene", "MainMenu")]
    public class MapStorageManager : SingletonBase<MapStorageManager>
    {
        private const string MAP_FILE_EXTENSION = ".happymap";
        private const string MAP_FOLDER = "Maps";
        private const int CURRENT_EDITOR_VERSION = 1;
        private readonly bool createFolderIfNotExist = true;

        private readonly bool useCustomPath = false;

        // 当前加载的地图数据
        private MapData currentMapData;
        private string customMapFolderPath = "Assets/StreamingAssets/Maps";

        private string MapFolderPath
        {
            get
            {
                // 优先使用 StreamingAssets 作为地图读取/保存路径，确保打包后可直接读取
                var streamingMapsPath = Path.Combine(Application.streamingAssetsPath, MAP_FOLDER);

                if (!string.IsNullOrEmpty(streamingMapsPath)) return streamingMapsPath;

                // 兜底：如果上面路径不可用（理论上在编辑器与Windows打包都可用），再根据自定义路径/持久化路径回退
                if (useCustomPath && !string.IsNullOrEmpty(customMapFolderPath))
                {
                    if (!Path.IsPathRooted(customMapFolderPath))
                        return Path.Combine(Application.dataPath, customMapFolderPath.Replace("Assets/", ""));
                    return customMapFolderPath;
                }

                return Path.Combine(Application.persistentDataPath, MAP_FOLDER);
            }
        }

        private void Start()
        {
            // 确保地图文件夹存在（编辑器中用于保存；打包后 StreamingAssets 已存在）
            EnsureMapFolderExists();
        }

        // 获取当前地图数据
        public MapData GetCurrentMapData()
        {
            return currentMapData;
        }

        private void EnsureMapFolderExists()
        {
            if (!createFolderIfNotExist) return;

            try
            {
                if (!Directory.Exists(MapFolderPath))
                {
                    Directory.CreateDirectory(MapFolderPath);
                    Debug.Log($"创建地图文件夹: {MapFolderPath}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"创建地图文件夹失败: {e.Message}");
            }
        }

        // 保存当前地图
        public void SaveMap(string mapName)
        {
            if (!MapManager.Instance)
            {
                Debug.LogError("MapStorageManager: MapManager引用丢失！");
                return;
            }

            if (!Directory.Exists(MapFolderPath))
            {
                if (createFolderIfNotExist)
                {
                    EnsureMapFolderExists();
                }
                else
                {
                    Debug.LogError($"地图文件夹不存在: {MapFolderPath}");
                    return;
                }
            }

            try
            {
                // 创建地图数据
                var mapData = new MapData(mapName, MapManager.Instance.GetMapSize());

                // 收集所有非空Tile的数据
                for (var x = 0; x < mapData.mapSize.x; x++)
                for (var y = 0; y < mapData.mapSize.y; y++)
                {
                    var tileInfo = MapManager.Instance.GetTile(x, y);
                    // 只保存非Empty的Tile
                    if (tileInfo.Type != TileType.Empty) mapData.tiles.Add(new SerializedTile(x, y, tileInfo.Type));
                }

                // 获取玩家初始位置和朝向
                var mainCharacterObject = GameObject.FindGameObjectWithTag("MainCharacter");
                if (mainCharacterObject)
                {
                    mapData.hasPlayer = true;
                    var gridObjectComponent = mainCharacterObject.GetComponent<BehaviorComponentContainer>()
                        .GetBehaviorComponent<GridObjectComponent>();
                    var directionComponent = mainCharacterObject.GetComponent<BehaviorComponentContainer>()
                        .GetBehaviorComponent<DirectionComponent>();
                    if (gridObjectComponent != null)
                    {
                        mapData.playerStartPosition = gridObjectComponent.GetGridPosition();
                    }
                    else
                    {
                        Debug.LogWarning("场景中的玩家对象没有GridObjectComponent，无法保存玩家初始位置。");
                        mapData.playerStartPosition = Vector2Int.zero; // 默认值
                    }

                    if (directionComponent != null)
                    {
                        mapData.playerStartDirection = directionComponent.GetDirection();
                    }
                    else
                    {
                        Debug.LogWarning("场景中的玩家对象没有DirectionComponent，无法保存玩家初始朝向。");
                        mapData.playerStartDirection = Direction.Right; // 默认值
                    }
                }
                else
                {
                    mapData.hasPlayer = false;
                    Debug.LogWarning("场景中未找到Tag为 'MainCharacter' 的玩家对象，地图将不包含玩家信息。");
                    mapData.playerStartPosition = Vector2Int.zero; // 或者一个预设的默认值
                    mapData.playerStartDirection = Direction.Right; // 默认朝向
                }

                // 获取装置信息（不再支持关卡出口装置）
                if (DeviceManager.Instance && GridObjectManager.Instance)
                {
                    var devices = GridObjectManager.Instance.GetObjectsOfType<DeviceBase>();
                    foreach (var device in devices)
                    {
                        var gridComp = device.GetBehaviorComponent<GridObjectComponent>();
                        var deviceTypeId = device.TypeId;
                        if (gridComp != null && deviceTypeId != null)
                        {
                            if (deviceTypeId.Id == "LevelExit") continue; // 跳过旧的关卡出口
                            mapData.devices.Add(new SerializedDevice(gridComp.GetGridPosition(), deviceTypeId.Id));
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("DeviceManager.Instance或GridObjectManager.Instance为空，无法保存装置信息。");
                }


                // 不再单独保存静态敌人，敌人信息由波次数据维护

                // 保留当前地图数据中的配置信息
                if (currentMapData != null)
                {
                    mapData.shouldEnterShop = currentMapData.shouldEnterShop;
                    // 波次配置保存（如在编辑器中有 MapWaveEditManager，则以其为准，将场景敌人写回当前波次）
                    if (MapWaveEditManager.Instance != null)
                    {
                        // 先将场景敌人写入当前波次
                        MapWaveEditManager.Instance.ApplySceneEnemiesToCurrentWave();
                        mapData.waves = MapWaveEditManager.Instance.GetWavesCopy();
                        mapData.totalWaves = MapWaveEditManager.Instance.TotalWaves;
                    }
                    else
                    {
                        mapData.waves = currentMapData.waves != null
                            ? new List<WaveConfig>(currentMapData.waves)
                            : new List<WaveConfig>();
                        mapData.totalWaves = mapData.waves.Count;
                    }
                    mapData.levelType = currentMapData.levelType;
                    mapData.levelDifficulty = currentMapData.levelDifficulty;
                }
                else
                {
                    // 如果没有当前地图数据，使用默认值
                    mapData.shouldEnterShop = false;
                    mapData.waves = new List<WaveConfig>();
                    mapData.totalWaves = 0;
                    mapData.levelType = LevelType.Normal;
                    mapData.levelDifficulty = 1;
                }


                // 转换为JSON
                var json = JsonUtility.ToJson(mapData, true);

                // 保存到文件
                var filePath = Path.Combine(MapFolderPath, mapName + MAP_FILE_EXTENSION);
                File.WriteAllText(filePath, json);

                Debug.Log($"地图已保存: {filePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"保存地图失败: {e.Message}");
            }
        }

        // 加载地图
        public bool LoadMap(string mapName)
        {
            if (!MapManager.Instance)
            {
                Debug.LogError("MapStorageManager: MapManager引用丢失！");
                return false;
            }

            try
            {
                var filePath = Path.Combine(MapFolderPath, mapName + MAP_FILE_EXTENSION);
                if (!File.Exists(filePath))
                {
                    Debug.LogError($"地图文件不存在: {filePath}");
                    return false;
                }

                // 读取JSON
                var json = File.ReadAllText(filePath);
                var mapData = JsonUtility.FromJson<MapData>(json);

                // 不再进行版本号检查或迁移（旧地图已弃用）

                // 清空当前地图（所有格子设为Empty）
                // 也需要清空场景中的旧角色、道具和敌人
                ClearExistingMapElements();

                MapManager.Instance.SetSize(mapData.mapSize.x, mapData.mapSize.y);

                for (var x = 0; x < MapManager.Instance.GetMapSize().x; x++)
                for (var y = 0; y < MapManager.Instance.GetMapSize().y; y++)
                    MapManager.Instance.SetTile(x, y, new TileInfo(TileType.Empty));

                // 根据MapData中的mapSize调整MapManager的尺寸 (如果需要)
                // MapManager.Instance.ResizeMap(mapData.mapSize.x, mapData.mapSize.y);
                // 应用读取的地图数据
                foreach (var tile in mapData.tiles)
                    MapManager.Instance.SetTile(tile.x, tile.y, new TileInfo(tile.type));

                // 更新地图显示
                MapManager.Instance.UpdateVisualMap();

                // 加载玩家
                if (mapData.hasPlayer && CharacterManager.Instance != null)
                {
                    // 假设有一个默认的玩家类型ID，例如 "DefaultPlayer"
                    // 你可能需要从其他地方获取这个ID，或者也将其存储在MapData中
                    var playerTypeId = TypeId.Create<CharacterTypeId>("Default");
                    var player =
                        CharacterController.Instance.CreateCharacter(playerTypeId, mapData.playerStartPosition);
                    if (player != null)
                    {
                        var directionComponent = player.GetBehaviorComponent<DirectionComponent>();
                        if (directionComponent != null) directionComponent.SetDirection(mapData.playerStartDirection);
                        // 确保玩家对象有 "MainCharacter" 标签，以便SaveMap时能找到
                        player.tag = "MainCharacter";
                    }
                }
                else if (!mapData.hasPlayer)
                {
                    Debug.Log("地图数据中不包含玩家信息，跳过玩家生成。");
                }
                else
                {
                    Debug.LogWarning("CharacterManager.Instance为空，无法加载玩家。");
                }

                // 加载装置（不再加载关卡出口）
                if (DeviceManager.Instance != null)
                    foreach (var serializedDevice in mapData.devices)
                    {
                        var deviceTypeId = TypeId.Create<DeviceTypeId>(serializedDevice.deviceType);
                        if (serializedDevice.deviceType == "LevelExit") continue;
                        DeviceController.Instance.PlaceDevice(serializedDevice.position, deviceTypeId);
                    }
                else
                    Debug.LogWarning("DeviceManager.Instance为空，无法加载装置。");

                // 在加载阶段生成第一波敌人（编辑器和游戏场景保持一致）
                if (EnemyManager.Instance != null)
                {
                    if (mapData.waves != null && mapData.waves.Count > 0)
                    {
                        var wave0 = mapData.waves[0];
                        if (wave0 != null && wave0.enemies != null)
                        {
                            foreach (var we in wave0.enemies)
                            {
                                if (string.IsNullOrEmpty(we.enemyTypeId)) continue;
                                var enemyTypeId = TypeId.Create<EnemyTypeId>(we.enemyTypeId);
                                EnemyController.Instance.CreateEnemy(enemyTypeId, we.position);
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("EnemyManager.Instance为空，无法加载第一波敌人。");
                }

                // 保存当前地图数据
                currentMapData = mapData;

                Debug.Log($"地图已加载: {filePath}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"加载地图失败: {e.Message}");
                return false;
            }
        }

        // 获取所有可用的地图
        public string[] GetAvailableMaps()
        {
            try
            {
                if (!Directory.Exists(MapFolderPath)) return new string[0];

                var files = Directory.GetFiles(MapFolderPath, "*" + MAP_FILE_EXTENSION);
                var mapNames = new List<string>();

                foreach (var file in files)
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    mapNames.Add(fileName);
                }

                return mapNames.ToArray();
            }
            catch (Exception e)
            {
                Debug.LogError($"获取地图列表失败: {e.Message}");
                return new string[0];
            }
        }

        // 新增：清空地图元素的方法
        private void ClearExistingMapElements()
        {
            // 清空GridObjectManager中的所有对象，这会一并处理角色、道具、敌人等
            if (GridObjectManager.Instance)
                GridObjectManager.Instance.ClearAllObjects();
            else
                Debug.LogWarning("GridObjectManager.Instance为空，可能无法完全清空地图元素。");
        }

        // 版本迁移逻辑已删除

        // 测试用方法，设置地图
        public void SetCurrentMapData(MapData mapData)
        {
            currentMapData = mapData;
        }

        // 更新当前地图的配置信息
        public void UpdateMapConfig(bool shouldEnterShop, List<WaveConfig> waves,
            LevelType levelType, int levelDifficulty)
        {
            if (currentMapData != null)
            {
                currentMapData.shouldEnterShop = shouldEnterShop;
                currentMapData.waves = new List<WaveConfig>(waves ?? new List<WaveConfig>());
                currentMapData.totalWaves = currentMapData.waves.Count;
                currentMapData.levelType = levelType;
                currentMapData.levelDifficulty = levelDifficulty;
            }
            else
            {
                Debug.LogWarning("当前没有加载的地图数据，无法更新配置。");
            }
        }

#if UNITY_EDITOR
        // 在Inspector中添加选择文件夹的按钮
        [CustomEditor(typeof(MapStorageManager))]
        public class MapStorageManagerEditor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();

                var manager = (MapStorageManager)target;

                EditorGUILayout.Space();
                if (GUILayout.Button("选择地图文件夹"))
                {
                    // 获取当前自定义路径的绝对路径作为起始目录
                    var startPath = manager.MapFolderPath;
                    var path = EditorUtility.OpenFolderPanel("选择地图保存位置", startPath, "");
                    if (!string.IsNullOrEmpty(path))
                    {
                        // 将绝对路径转换为相对于Assets的路径
                        if (path.StartsWith(Application.dataPath))
                            manager.customMapFolderPath = "Assets" + path.Substring(Application.dataPath.Length);
                        else
                            // 如果不是在Assets目录下，保持绝对路径
                            manager.customMapFolderPath = path;
                        EditorUtility.SetDirty(manager);
                    }
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("当前地图保存路径:", manager.MapFolderPath);
            }
        }
#endif
    }
}