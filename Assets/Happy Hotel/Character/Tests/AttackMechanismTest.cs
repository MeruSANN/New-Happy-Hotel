using System.Collections;
using HappyHotel.Character;
using HappyHotel.Character.Components;
using HappyHotel.Core;
using HappyHotel.Core.Grid.Components;
using HappyHotel.Core.Registry;
using HappyHotel.Core.Singleton;
using HappyHotel.Core.ValueProcessing.Components;
using HappyHotel.Enemy;
using HappyHotel.GameManager;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Tilemaps;
using CharacterController = HappyHotel.Character.CharacterController;

public class AttackMechanismTest
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
    private EnemyBase testEnemy;

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
        if (testEnemy != null)
            Object.DestroyImmediate(testEnemy.gameObject);
        Object.DestroyImmediate(playerObject);
        Object.DestroyImmediate(gridObject);

        // 再清理单例
        singletonManager.ClearSingletonsImmediate();
        Object.DestroyImmediate(singletonManagerObj);
    }

    [UnityTest]
    public IEnumerator Character_ShouldHaveAttackComponents_WhenCreated()
    {
        // 等待一帧让Start方法执行
        yield return null;

        // 验证角色是否有攻击力组件
        var attackPowerComponent = defaultCharacter.GetBehaviorComponent<AttackPowerComponent>();
        Assert.IsNotNull(attackPowerComponent, "角色应该有AttackPowerComponent组件");

        // 验证角色是否有攻击组件
        var attackComponent = defaultCharacter.GetBehaviorComponent<AttackComponent>();
        Assert.IsNotNull(attackComponent, "角色应该有AttackComponent组件");

        // 验证攻击力组件是否正常工作
        var attackPower = attackPowerComponent.GetAttackPower();
        Assert.GreaterOrEqual(attackPower, 0, "攻击力应该大于等于0");
    }

    [UnityTest]
    public IEnumerator Character_ShouldAttackEnemy_WhenMovingIntoEnemyPosition()
    {
        // 等待一帧让Start方法执行
        yield return null;

        // 创建测试敌人在玩家右侧一格位置
        var enemyPosition = new Vector2Int(1, 0);
        testEnemy = EnemyController.Instance.CreateEnemy("TestEnemy", enemyPosition);
        Assert.IsNotNull(testEnemy, "应该成功创建测试敌人");

        // 获取敌人的初始血量
        var enemyHealthComponent = testEnemy.GetBehaviorComponent<HitPointValueComponent>();
        Assert.IsNotNull(enemyHealthComponent, "敌人应该有血量组件");
        var initialHealth = enemyHealthComponent.CurrentHitPoint;

        // 获取角色的攻击力
        var attackPowerComponent = defaultCharacter.GetBehaviorComponent<AttackPowerComponent>();
        var attackPower = attackPowerComponent.GetAttackPower();

        // 设置为播放状态，让角色开始移动
        gameManager.SetGameState(GameManager.GameState.Playing);

        // 等待角色移动到敌人位置并执行攻击
        yield return new WaitForSeconds(moveInterval + waitBuffer);

        // 验证敌人是否受到伤害
        var finalHealth = enemyHealthComponent.CurrentHitPoint;
        var expectedHealth = initialHealth - attackPower;
        Assert.AreEqual(expectedHealth, finalHealth, $"敌人应该受到 {attackPower} 点伤害，从 {initialHealth} 降到 {expectedHealth}，实际血量: {finalHealth}");

        // 验证角色位置是否正确（应该在敌人位置）
        var expectedTilePosition = new Vector3Int(1, 0, 0);
        var actualTilePosition = grid.WorldToCell(playerObject.transform.position);
        Assert.AreEqual(expectedTilePosition, actualTilePosition, "角色应该移动到敌人位置");
    }

    [UnityTest]
    public IEnumerator Character_ShouldNotAttackEnemy_WhenEnemyIsNotInTargetTags()
    {
        // 等待一帧让Start方法执行
        yield return null;

        // 创建测试敌人在玩家右侧一格位置
        var enemyPosition = new Vector2Int(1, 0);
        testEnemy = EnemyController.Instance.CreateEnemy("TestEnemy", enemyPosition);
        Assert.IsNotNull(testEnemy, "应该成功创建测试敌人");

        // 移除敌人的"Enemy"标签，添加其他标签
        testEnemy.RemoveTag("Enemy");
        testEnemy.AddTag("Friendly");

        // 获取敌人的初始血量
        var enemyHealthComponent = testEnemy.GetBehaviorComponent<HitPointValueComponent>();
        var initialHealth = enemyHealthComponent.CurrentHitPoint;

        // 设置为播放状态，让角色开始移动
        gameManager.SetGameState(GameManager.GameState.Playing);

        // 等待角色移动到敌人位置
        yield return new WaitForSeconds(moveInterval + waitBuffer);

        // 验证敌人没有受到伤害
        var finalHealth = enemyHealthComponent.CurrentHitPoint;
        Assert.AreEqual(initialHealth, finalHealth, "敌人不应该受到伤害，因为不是目标标签");

        // 验证角色位置是否正确（应该在敌人位置）
        var expectedTilePosition = new Vector3Int(1, 0, 0);
        var actualTilePosition = grid.WorldToCell(playerObject.transform.position);
        Assert.AreEqual(expectedTilePosition, actualTilePosition, "角色应该移动到敌人位置");
    }

    [UnityTest]
    public IEnumerator Character_ShouldAttackAllEnemies_WhenMultipleEnemiesInSamePosition()
    {
        // 等待一帧让Start方法执行
        yield return null;

        // 创建两个测试敌人在玩家右侧一格位置
        var enemyPosition = new Vector2Int(1, 0);
        var enemy1 = EnemyController.Instance.CreateEnemy("TestEnemy", enemyPosition);
        var enemy2 = EnemyController.Instance.CreateEnemy("TestEnemy", enemyPosition);
        Assert.IsNotNull(enemy1, "应该成功创建第一个测试敌人");
        Assert.IsNotNull(enemy2, "应该成功创建第二个测试敌人");

        // 获取敌人的初始血量
        var enemy1HealthComponent = enemy1.GetBehaviorComponent<HitPointValueComponent>();
        var enemy2HealthComponent = enemy2.GetBehaviorComponent<HitPointValueComponent>();
        var initialHealth1 = enemy1HealthComponent.CurrentHitPoint;
        var initialHealth2 = enemy2HealthComponent.CurrentHitPoint;

        // 获取角色的攻击力
        var attackPowerComponent = defaultCharacter.GetBehaviorComponent<AttackPowerComponent>();
        var attackPower = attackPowerComponent.GetAttackPower();

        // 设置为播放状态，让角色开始移动
        gameManager.SetGameState(GameManager.GameState.Playing);

        // 等待角色移动到敌人位置并执行攻击
        yield return new WaitForSeconds(moveInterval + waitBuffer);

        // 验证两个敌人都受到伤害
        var finalHealth1 = enemy1HealthComponent.CurrentHitPoint;
        var finalHealth2 = enemy2HealthComponent.CurrentHitPoint;
        var expectedHealth1 = initialHealth1 - attackPower;
        var expectedHealth2 = initialHealth2 - attackPower;

        Assert.AreEqual(expectedHealth1, finalHealth1, $"第一个敌人应该受到 {attackPower} 点伤害");
        Assert.AreEqual(expectedHealth2, finalHealth2, $"第二个敌人应该受到 {attackPower} 点伤害");

        // 清理额外创建的敌人
        Object.DestroyImmediate(enemy1.gameObject);
        Object.DestroyImmediate(enemy2.gameObject);
    }

    [UnityTest]
    public IEnumerator Character_ShouldNotAttackSelf_WhenMovingToOwnPosition()
    {
        // 等待一帧让Start方法执行
        yield return null;

        // 获取角色的初始血量
        var characterHealthComponent = defaultCharacter.GetBehaviorComponent<HitPointValueComponent>();
        var initialHealth = characterHealthComponent.CurrentHitPoint;

        // 获取角色的攻击力
        var attackPowerComponent = defaultCharacter.GetBehaviorComponent<AttackPowerComponent>();
        var attackPower = attackPowerComponent.GetAttackPower();

        // 设置为播放状态，让角色开始移动
        gameManager.SetGameState(GameManager.GameState.Playing);

        // 等待角色移动
        yield return new WaitForSeconds(moveInterval + waitBuffer);

        // 验证角色没有受到伤害（不应该攻击自己）
        var finalHealth = characterHealthComponent.CurrentHitPoint;
        Assert.AreEqual(initialHealth, finalHealth, "角色不应该攻击自己，血量应该保持不变");

        // 验证角色位置是否正确（应该向右移动一格）
        var expectedTilePosition = new Vector3Int(1, 0, 0);
        var actualTilePosition = grid.WorldToCell(playerObject.transform.position);
        Assert.AreEqual(expectedTilePosition, actualTilePosition, "角色应该向右移动一格");
    }

    [UnityTest]
    public IEnumerator Character_ShouldAttackEnemy_WhenEnemyMovesIntoCharacterPosition()
    {
        // 等待一帧让Start方法执行
        yield return null;

        // 创建测试敌人在玩家右侧一格位置
        var enemyPosition = new Vector2Int(1, 0);
        testEnemy = EnemyController.Instance.CreateEnemy("TestEnemy", enemyPosition);
        Assert.IsNotNull(testEnemy, "应该成功创建测试敌人");

        // 获取敌人的初始血量
        var enemyHealthComponent = testEnemy.GetBehaviorComponent<HitPointValueComponent>();
        var initialHealth = enemyHealthComponent.CurrentHitPoint;

        // 获取角色的攻击力
        var attackPowerComponent = defaultCharacter.GetBehaviorComponent<AttackPowerComponent>();
        var attackPower = attackPowerComponent.GetAttackPower();

        // 让敌人移动到角色位置
        var enemyGridComponent = testEnemy.GetBehaviorComponent<GridObjectComponent>();
        enemyGridComponent.MoveTo(Vector2Int.zero);

        // 等待一帧让攻击逻辑执行
        yield return null;

        // 验证敌人是否受到伤害
        var finalHealth = enemyHealthComponent.CurrentHitPoint;
        var expectedHealth = initialHealth - attackPower;
        Assert.AreEqual(expectedHealth, finalHealth, $"敌人应该受到 {attackPower} 点伤害，从 {initialHealth} 降到 {expectedHealth}，实际血量: {finalHealth}");
    }
}
