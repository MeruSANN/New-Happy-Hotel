using System.Collections;
using HappyHotel.Character;
using HappyHotel.Core.Grid;
using HappyHotel.Core.Grid.Components;
using HappyHotel.Core.Registry;
using HappyHotel.Core.Singleton;
using HappyHotel.Device;
using HappyHotel.Map;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Tilemaps;
using CharacterController = HappyHotel.Character.CharacterController;

public class RockDeviceTest
{
    private readonly float moveInterval = 0.5f;
    private DefaultCharacter defaultCharacter;
    private Grid grid;
    private GameObject gridObj;
    private Tilemap mapTilemap;
    private GameObject playerObj;
    private SingletonManager singletonManager;
    private GameObject singletonManagerObj;
    private GameObject tilemapObj;

    [SetUp]
    public void Setup()
    {
        // 创建网格对象
        gridObj = new GameObject("Grid")
        {
            tag = "Grid"
        };
        grid = gridObj.AddComponent<Grid>();
        grid.cellSize = new Vector3(1, 1, 0);

        // 创建Map Tilemap
        tilemapObj = new GameObject("Tilemap");
        tilemapObj.transform.SetParent(gridObj.transform);
        tilemapObj.tag = "MapTilemap";
        tilemapObj.AddComponent<Tilemap>();
        tilemapObj.AddComponent<TilemapRenderer>();

        // 创建单例管理器
        singletonManagerObj = new GameObject("Singleton Manager");
        singletonManager = singletonManagerObj.AddComponent<SingletonManager>();

        // 创建玩家对象
        var characterTypeId = TypeId.Create<CharacterTypeId>("Default");
        defaultCharacter =
            (DefaultCharacter)CharacterController.Instance.CreateCharacter(characterTypeId, Vector2Int.zero);
        playerObj = defaultCharacter.gameObject;
        defaultCharacter.GetBehaviorComponent<AutoMoveComponent>().SetMoveInterval(moveInterval);
    }

    [TearDown]
    public void TearDown()
    {
        // 先销毁游戏对象，避免单例销毁时影响到对象引用
        Object.DestroyImmediate(playerObj);
        Object.DestroyImmediate(gridObj);
        Object.DestroyImmediate(tilemapObj);

        // 再清理单例
        singletonManager.ClearSingletonsImmediate();
        Object.DestroyImmediate(singletonManagerObj);
    }

    [UnityTest]
    public IEnumerator TestRockDeviceBlocksMovement()
    {
        // 等待一帧让Start方法执行
        yield return null;

        // 在(1,0)位置放置石头
        var rockTypeId = TypeId.Create<DeviceTypeId>("Rock");
        var rockDevice = DeviceController.Instance.PlaceDevice(new Vector2Int(1, 0), rockTypeId);
        Assert.IsNotNull(rockDevice, "石头应该成功创建");

        // 验证石头实现了IBlockingDevice接口
        Assert.IsTrue(rockDevice is IBlockingDevice, "石头应该实现IBlockingDevice接口");

        // 设置玩家初始位置
        GridObjectManager.Instance.MoveObject(defaultCharacter, Vector2Int.zero);

        // 验证石头位置被MapManager识别为墙体
        Assert.IsTrue(MapManager.Instance.IsWall(1, 0), "石头位置应该被识别为墙体");
        Assert.IsFalse(MapManager.Instance.IsWalkable(1, 0), "石头位置应该不可通行");

        // 测试玩家无法移动到石头位置
        var canMoveToRock = GridObjectManager.Instance.MoveObject(defaultCharacter, new Vector2Int(1, 0));
        Assert.IsFalse(canMoveToRock, "玩家不应该能移动到石头位置");
        Assert.AreEqual(new Vector2Int(0, 0),
            defaultCharacter.GetBehaviorComponent<GridObjectComponent>().GetGridPosition(), "玩家位置不应该改变");
    }

    [Test]
    public void TestRockDeviceInCornerDetection()
    {
        // 创建一个角落场景：左边是墙，上边是石头
        MapManager.Instance.SetTile(0, 1, new TileInfo(TileType.Wall)); // 左

        // 在上方放置石头
        var rockTypeId = TypeId.Create<DeviceTypeId>("Rock");
        var rockDevice = DeviceController.Instance.PlaceDevice(new Vector2Int(1, 2), rockTypeId);
        Assert.IsNotNull(rockDevice, "石头应该成功创建");

        // 验证角落检测（石头被算作墙面）
        var isCorner = MapManager.Instance.IsCorner(1, 1);
        Assert.IsTrue(isCorner, "位置(1,1)应该被识别为角落（左墙+上石头）");

        // 验证排除逻辑
        var shouldExclude = MapManager.Instance.ShouldExcludeFromPropSpawn(1, 1);
        Assert.IsTrue(shouldExclude, "角落位置应该被排除在Prop刷新之外");

        // 清理
        MapManager.Instance.SetTile(0, 1, new TileInfo(TileType.Floor));
        DeviceController.Instance.RemoveDevice(new Vector2Int(1, 2));
    }

    [Test]
    public void TestRockDeviceThreeWallDetection()
    {
        // 创建一个三面"墙"场景：上墙、左墙、右石头
        MapManager.Instance.SetTile(1, 2, new TileInfo(TileType.Wall)); // 上
        MapManager.Instance.SetTile(0, 1, new TileInfo(TileType.Wall)); // 左

        // 在右边放置石头
        var rockTypeId = TypeId.Create<DeviceTypeId>("Rock");
        var rockDevice = DeviceController.Instance.PlaceDevice(new Vector2Int(2, 1), rockTypeId);
        Assert.IsNotNull(rockDevice, "石头应该成功创建");

        // 验证三面墙检测（包括石头）
        var hasThreeWalls = MapManager.Instance.HasThreeOrMoreWalls(1, 1);
        Assert.IsTrue(hasThreeWalls, "位置(1,1)应该被识别为有三面墙（包括石头）");

        // 验证排除逻辑
        var shouldExclude = MapManager.Instance.ShouldExcludeFromPropSpawn(1, 1);
        Assert.IsTrue(shouldExclude, "三面墙位置应该被排除在Prop刷新之外");

        // 清理
        MapManager.Instance.SetTile(1, 2, new TileInfo(TileType.Floor));
        MapManager.Instance.SetTile(0, 1, new TileInfo(TileType.Floor));
        DeviceController.Instance.RemoveDevice(new Vector2Int(2, 1));
    }

    [Test]
    public void TestRockDeviceIsDestructible()
    {
        // 创建石头Device
        var rockTypeId = TypeId.Create<DeviceTypeId>("Rock");
        var rockDevice = DeviceController.Instance.PlaceDevice(new Vector2Int(3, 3), rockTypeId);
        Assert.IsNotNull(rockDevice, "石头应该成功创建");

        // 验证石头不可破坏
        var blockingDevice = rockDevice as IBlockingDevice;
        Assert.IsNotNull(blockingDevice, "石头应该实现IBlockingDevice接口");
        Assert.IsFalse(blockingDevice.IsDestructible, "石头应该不可破坏");
    }
}