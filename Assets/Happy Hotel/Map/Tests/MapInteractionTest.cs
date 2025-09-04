using System.Collections;
using HappyHotel.Character;
using HappyHotel.Core;
using HappyHotel.Core.Grid;
using HappyHotel.Core.Grid.Components;
using HappyHotel.Core.Registry;
using HappyHotel.Core.Singleton;
using HappyHotel.GameManager;
using HappyHotel.Map;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Tilemaps;
using CharacterController = HappyHotel.Character.CharacterController;


public class MapInteractionTest
{
    private readonly float moveInterval = 0.5f; // 移动间隔时间
    private readonly float waitBuffer = 0.1f; // 等待缓冲时间
    private DefaultCharacter defaultCharacter;
    private GameManager gameManager;
    private Grid grid;
    private GameObject gridObject;
    private GameObject playerObject;
    private SingletonManager singletonManager;
    private GameObject singletonManagerObj;
    private GameObject tilemapObject;

    [SetUp]
    public void Setup()
    {
        // 创建网格对象
        gridObject = new GameObject("Grid");
        gridObject.tag = "Grid";
        grid = gridObject.AddComponent<Grid>();
        grid.cellSize = new Vector3(1, 1, 0);

        // 创建Map Tilemap
        tilemapObject = new GameObject("Tilemap");
        tilemapObject.transform.SetParent(gridObject.transform);
        tilemapObject.tag = "MapTilemap";
        tilemapObject.AddComponent<Tilemap>();
        tilemapObject.AddComponent<TilemapRenderer>();

        // 创建单例管理器
        singletonManagerObj = new GameObject("Singleton Manager");
        singletonManager = singletonManagerObj.AddComponent<SingletonManager>();

        // 创建游戏状态控制器
        gameManager = GameManager.Instance;

        // 创建玩家对象
        var typeId = TypeId.Create<CharacterTypeId>("Default");
        defaultCharacter = (DefaultCharacter)CharacterController.Instance.CreateCharacter(typeId, Vector2Int.zero);
        playerObject = defaultCharacter.gameObject;
        defaultCharacter.GetBehaviorComponent<AutoMoveComponent>().SetMoveInterval(moveInterval);

        GridObjectManager.Instance.MoveObject(defaultCharacter, Vector2Int.zero);
    }

    [TearDown]
    public void TearDown()
    {
        // 先销毁游戏对象，避免单例销毁时影响到对象引用
        Object.DestroyImmediate(playerObject);
        Object.DestroyImmediate(gridObject);

        // 再清理单例
        singletonManager.ClearSingletonsImmediate();
        Object.DestroyImmediate(singletonManagerObj);
    }

    [UnityTest]
    public IEnumerator Player_CantMoveThroughWall_Right()
    {
        TurnManager.Instance.StartFirstTurn();

        // 等待一帧让Start方法执行
        yield return null;

        // 在玩家右侧放置墙体
        var wallPosition = new Vector2Int(1, 0);
        MapManager.Instance.SetTile(wallPosition.x, wallPosition.y, new TileInfo(TileType.Wall));

        // 记录初始位置
        var initialPosition = grid.WorldToCell(playerObject.transform.position);

        // 设置为播放状态
        gameManager.SetGameState(GameManager.GameState.Playing);

        // 等待一次移动间隔
        yield return new WaitForSeconds(moveInterval + waitBuffer);

        // 验证玩家位置没有改变（被墙挡住了）
        var currentPosition = grid.WorldToCell(playerObject.transform.position);
        Assert.AreEqual(initialPosition, currentPosition);

        // 验证游戏状态是否回到待机状态
        Assert.AreEqual(GameManager.GameState.Idle, gameManager.GetGameState());
    }

    [UnityTest]
    public IEnumerator Player_CantMoveThroughWall_AllDirections()
    {
        TurnManager.Instance.StartFirstTurn();

        // 等待一帧让Start方法执行
        yield return null;

        // 在玩家四周放置墙体
        MapManager.Instance.SetTile(1, 0, new TileInfo(TileType.Wall)); // 右
        MapManager.Instance.SetTile(0, 1, new TileInfo(TileType.Wall)); // 上
        MapManager.Instance.SetTile(-1, 0, new TileInfo(TileType.Wall)); // 左
        MapManager.Instance.SetTile(0, -1, new TileInfo(TileType.Wall)); // 下

        // 记录初始位置
        var initialPosition = grid.WorldToCell(playerObject.transform.position);

        // 测试所有方向
        var directions = new[]
        {
            Direction.Right,
            Direction.Up,
            Direction.Left,
            Direction.Down
        };

        foreach (var direction in directions)
        {
            // 设置方向
            var directionComponent = defaultCharacter.GetBehaviorComponent<DirectionComponent>();
            Assert.IsNotNull(directionComponent, "玩家应该有DirectionComponent组件");
            directionComponent.SetDirection(direction);

            // 设置为播放状态
            gameManager.SetGameState(GameManager.GameState.Playing);

            // 等待一次移动间隔
            yield return new WaitForSeconds(moveInterval + waitBuffer);

            // 验证玩家位置没有改变（被墙挡住了）
            var currentPosition = grid.WorldToCell(playerObject.transform.position);
            Assert.AreEqual(initialPosition, currentPosition);

            // 重置为待机状态
            gameManager.SetGameState(GameManager.GameState.Idle);
        }
    }

    [Test]
    public void MapManager_CornerDetection_WorksCorrectly()
    {
        // 创建一个角落场景：左上角
        MapManager.Instance.SetTile(1, 2, new TileInfo(TileType.Wall)); // 上
        MapManager.Instance.SetTile(0, 1, new TileInfo(TileType.Wall)); // 左

        // 验证角落检测
        var isCorner = MapManager.Instance.IsCorner(1, 1);
        Assert.IsTrue(isCorner, "位置(1,1)应该被识别为角落");

        // 验证排除逻辑
        var shouldExclude = MapManager.Instance.ShouldExcludeFromPropSpawn(1, 1);
        Assert.IsTrue(shouldExclude, "角落位置应该被排除在Prop刷新之外");

        // 清理
        MapManager.Instance.SetTile(1, 2, new TileInfo(TileType.Floor));
        MapManager.Instance.SetTile(0, 1, new TileInfo(TileType.Floor));
    }

    [Test]
    public void MapManager_ThreeWallDetection_WorksCorrectly()
    {
        // 创建一个三面墙场景
        MapManager.Instance.SetTile(1, 2, new TileInfo(TileType.Wall)); // 上
        MapManager.Instance.SetTile(0, 1, new TileInfo(TileType.Wall)); // 左
        MapManager.Instance.SetTile(2, 1, new TileInfo(TileType.Wall)); // 右

        // 验证三面墙检测
        var hasThreeWalls = MapManager.Instance.HasThreeOrMoreWalls(1, 1);
        Assert.IsTrue(hasThreeWalls, "位置(1,1)应该被识别为有三面墙");

        // 验证排除逻辑
        var shouldExclude = MapManager.Instance.ShouldExcludeFromPropSpawn(1, 1);
        Assert.IsTrue(shouldExclude, "三面墙位置应该被排除在Prop刷新之外");

        // 清理
        MapManager.Instance.SetTile(1, 2, new TileInfo(TileType.Floor));
        MapManager.Instance.SetTile(0, 1, new TileInfo(TileType.Floor));
        MapManager.Instance.SetTile(2, 1, new TileInfo(TileType.Floor));
    }

    [Test]
    public void MapManager_OppositeWallsNotCorner_WorksCorrectly()
    {
        // 创建对面墙场景（上下对面）
        MapManager.Instance.SetTile(1, 2, new TileInfo(TileType.Wall)); // 上
        MapManager.Instance.SetTile(1, 0, new TileInfo(TileType.Wall)); // 下

        // 验证不是角落
        var isCorner = MapManager.Instance.IsCorner(1, 1);
        Assert.IsFalse(isCorner, "对面墙不应该被识别为角落");

        // 验证不被排除
        var shouldExclude = MapManager.Instance.ShouldExcludeFromPropSpawn(1, 10);
        Assert.IsFalse(shouldExclude, "对面墙位置不应该被排除在Prop刷新之外");

        // 清理
        MapManager.Instance.SetTile(1, 2, new TileInfo(TileType.Floor));
        MapManager.Instance.SetTile(1, 0, new TileInfo(TileType.Floor));

        // 测试左右对面墙
        MapManager.Instance.SetTile(0, 1, new TileInfo(TileType.Wall)); // 左
        MapManager.Instance.SetTile(2, 1, new TileInfo(TileType.Wall)); // 右

        // 验证不是角落
        isCorner = MapManager.Instance.IsCorner(1, 1);
        Assert.IsFalse(isCorner, "左右对面墙不应该被识别为角落");

        // 验证不被排除
        shouldExclude = MapManager.Instance.ShouldExcludeFromPropSpawn(1, 1);
        Assert.IsFalse(shouldExclude, "左右对面墙位置不应该被排除在Prop刷新之外");

        // 清理
        MapManager.Instance.SetTile(0, 1, new TileInfo(TileType.Floor));
        MapManager.Instance.SetTile(2, 1, new TileInfo(TileType.Floor));
    }

    [Test]
    public void MapManager_NormalPosition_NotExcluded()
    {
        // 创建一个正常位置（只有一面墙或没有墙）
        MapManager.Instance.SetTile(1, 2, new TileInfo(TileType.Wall)); // 只有上面一面墙

        // 验证不是角落
        var isCorner = MapManager.Instance.IsCorner(1, 1);
        Assert.IsFalse(isCorner, "只有一面墙不应该被识别为角落");

        // 验证没有三面墙
        var hasThreeWalls = MapManager.Instance.HasThreeOrMoreWalls(1, 1);
        Assert.IsFalse(hasThreeWalls, "只有一面墙不应该被识别为三面墙");

        // 验证不被排除
        var shouldExclude = MapManager.Instance.ShouldExcludeFromPropSpawn(1, 1);
        Assert.IsFalse(shouldExclude, "正常位置不应该被排除在Prop刷新之外");

        // 清理
        MapManager.Instance.SetTile(1, 2, new TileInfo(TileType.Floor));
    }

    private Direction GetDirectionToPosition(Vector3Int current, Vector3Int target)
    {
        var diff = target - current;
        if (diff.x > 0) return Direction.Right;
        if (diff.x < 0) return Direction.Left;
        if (diff.y > 0) return Direction.Up;
        if (diff.y < 0) return Direction.Down;
        return Direction.Right; // 默认值
    }
}