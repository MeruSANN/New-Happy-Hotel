using System.Collections;
using HappyHotel.Character;
using HappyHotel.Core;
using HappyHotel.Core.Grid.Components;
using HappyHotel.Core.Registry;
using HappyHotel.Core.Singleton;
using HappyHotel.GameManager;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Tilemaps;
using CharacterController = HappyHotel.Character.CharacterController;


public class PlayerTest
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
        gridObject = new GameObject("Grid")
        {
            tag = "Grid"
        };
        grid = gridObject.AddComponent<Grid>();
        grid.cellSize = new Vector3(1, 1, 0);

        // 创建单例管理器
        singletonManagerObj = new GameObject("Singleton Manager");
        singletonManager = singletonManagerObj.AddComponent<SingletonManager>();

        // 创建Map Tilemap
        tilemapObject = new GameObject("Tilemap");
        tilemapObject.transform.SetParent(gridObject.transform);
        tilemapObject.tag = "MapTilemap";
        tilemapObject.AddComponent<Tilemap>();
        tilemapObject.AddComponent<TilemapRenderer>();

        gameManager = GameManager.Instance;

        // 创建玩家对象
        var typeId = TypeId.Create<CharacterTypeId>("Default");
        defaultCharacter = (DefaultCharacter)CharacterController.Instance.CreateCharacter(typeId, Vector2Int.zero);
        playerObject = defaultCharacter.gameObject;
        defaultCharacter.GetBehaviorComponent<AutoMoveComponent>().SetMoveInterval(moveInterval);
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
    public IEnumerator PlayerInitialPosition_ShouldBeZero()
    {
        // 等待一帧让Start方法执行
        yield return null;

        // 验证玩家初始Tile位置是否为(0,0)
        var expectedTilePosition = Vector3Int.zero;
        var actualTilePosition = grid.WorldToCell(playerObject.transform.position);
        Assert.AreEqual(expectedTilePosition, actualTilePosition);
    }

    [UnityTest]
    public IEnumerator PlayerMoves_RightDirection_AfterInterval_WhenPlaying()
    {
        // 等待一帧让Start方法执行
        yield return null;

        // 记录初始Tile位置
        var initialTilePosition = grid.WorldToCell(playerObject.transform.position);

        // 设置为播放状态
        gameManager.SetGameState(GameManager.GameState.Playing);

        // 等待超过移动间隔的时间
        yield return new WaitForSeconds(moveInterval + waitBuffer);

        // 验证玩家是否向右移动了一格
        var expectedTilePosition = initialTilePosition + new Vector3Int(1, 0, 0);
        var actualTilePosition = grid.WorldToCell(playerObject.transform.position);
        Assert.AreEqual(expectedTilePosition, actualTilePosition);
    }

    [UnityTest]
    public IEnumerator PlayerDoesNotMove_WhenIdle()
    {
        // 等待一帧让Start方法执行
        yield return null;

        // 记录初始Tile位置
        var initialTilePosition = grid.WorldToCell(playerObject.transform.position);

        // 确保是待机状态（默认状态）
        Assert.AreEqual(GameManager.GameState.Idle, gameManager.GetGameState());

        // 等待超过移动间隔的时间
        yield return new WaitForSeconds(moveInterval + waitBuffer);

        // 验证玩家位置没有改变
        var actualTilePosition = grid.WorldToCell(playerObject.transform.position);
        Assert.AreEqual(initialTilePosition, actualTilePosition);
    }

    [UnityTest]
    public IEnumerator PlayerMoves_UpDirection_AfterChangingDirection_WhenPlaying()
    {
        // 等待一帧让Start方法执行
        yield return null;

        // 改变方向为向上
        var directionComponent = defaultCharacter.GetBehaviorComponent<DirectionComponent>();
        Assert.IsNotNull(directionComponent, "玩家应该有DirectionComponent组件");
        directionComponent.SetDirection(Direction.Up);

        // 设置为播放状态
        gameManager.SetGameState(GameManager.GameState.Playing);

        // 记录初始Tile位置
        var initialTilePosition = grid.WorldToCell(playerObject.transform.position);

        // 等待超过移动间隔的时间
        yield return new WaitForSeconds(moveInterval + waitBuffer);

        // 验证玩家是否向上移动了一格
        var expectedTilePosition = initialTilePosition + new Vector3Int(0, 1, 0);
        var actualTilePosition = grid.WorldToCell(playerObject.transform.position);
        Assert.AreEqual(expectedTilePosition, actualTilePosition);
    }

    [UnityTest]
    public IEnumerator PlayerMoves_MultipleTimes_InSameDirection_WhenPlaying()
    {
        // 等待一帧让Start方法执行
        yield return null;

        // 设置为播放状态
        gameManager.SetGameState(GameManager.GameState.Playing);

        // 记录初始Tile位置
        var initialTilePosition = grid.WorldToCell(playerObject.transform.position);

        // 等待足够时间让玩家移动多次（例如3次）
        yield return new WaitForSeconds(3 * moveInterval + waitBuffer);

        // 验证玩家是否向右移动了3格
        var expectedTilePosition = initialTilePosition + new Vector3Int(3, 0, 0);
        var actualTilePosition = grid.WorldToCell(playerObject.transform.position);
        Assert.AreEqual(expectedTilePosition, actualTilePosition);
    }
}