using System.Collections;
using HappyHotel.Character;
using HappyHotel.Core;
using HappyHotel.Core.Grid.Components;
using HappyHotel.Core.Registry;
using HappyHotel.Core.Singleton;
using HappyHotel.Core.ValueProcessing.Components;
using HappyHotel.GameManager;
using HappyHotel.Prop;
using HappyHotel.Prop.Components;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Tilemaps;
using CharacterController = HappyHotel.Character.CharacterController;

public class IronSwordPropTest
{
    private DefaultCharacter player;
    private IronSwordProp ironSwordProp;
    private GameManager gameManager;
    private Grid grid;
    private GameObject gridObject;
    private GameObject playerObject;
    private GameObject propObject;
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
        player = (DefaultCharacter)CharacterController.Instance.CreateCharacter(typeId, Vector2Int.zero);
        playerObject = player.gameObject;

        // 创建铁剑道具
        var propTypeId = TypeId.Create<PropTypeId>("IronSword");
        ironSwordProp = (IronSwordProp)PropController.Instance.PlaceProp(new Vector2Int(1, 0), propTypeId);
        propObject = ironSwordProp.gameObject;
    }

    [TearDown]
    public void TearDown()
    {
        // 先销毁游戏对象，避免单例销毁时影响到对象引用
        Object.DestroyImmediate(playerObject);
        Object.DestroyImmediate(propObject);
        Object.DestroyImmediate(gridObject);

        // 再清理单例
        singletonManager.ClearSingletonsImmediate();
        Object.DestroyImmediate(singletonManagerObj);
    }

    [UnityTest]
    public IEnumerator IronSwordProp_ShouldHaveAttackPowerBoosterComponent()
    {
        // 等待一帧让Awake方法执行
        yield return null;

        // 验证铁剑道具应该有AttackPowerBoosterComponent
        var attackPowerBooster = ironSwordProp.GetBehaviorComponent<AttackPowerBoosterComponent>();
        Assert.IsNotNull(attackPowerBooster, "铁剑道具应该有AttackPowerBoosterComponent");
    }

    [UnityTest]
    public IEnumerator IronSwordProp_ShouldConfigureAttackPowerBonus()
    {
        // 等待一帧让Awake方法执行
        yield return null;

        // 验证AttackPowerBoosterComponent已配置攻击力加成
        var attackPowerBooster = ironSwordProp.GetBehaviorComponent<AttackPowerBoosterComponent>();
        Assert.IsNotNull(attackPowerBooster, "AttackPowerBoosterComponent应该存在");
        
        // 获取当前配置的攻击力加成
        var bonus = attackPowerBooster.GetAttackPowerBonus();
        Assert.Greater(bonus, 0, "攻击力加成应该大于0");
    }

    [UnityTest]
    public IEnumerator IronSwordProp_ShouldIncreasePlayerAttackPower_WhenPlayerMovesToProp()
    {
        // 等待一帧让Awake方法执行
        yield return null;

        // 获取玩家初始攻击力
        var attackPowerComponent = player.GetBehaviorComponent<AttackPowerComponent>();
        Assert.IsNotNull(attackPowerComponent, "玩家应该有AttackPowerComponent");
        
        var initialAttackPower = attackPowerComponent.GetAttackPower();
        Debug.Log($"玩家初始攻击力: {initialAttackPower}");

        // 获取铁剑的攻击力加成
        var attackPowerBooster = ironSwordProp.GetBehaviorComponent<AttackPowerBoosterComponent>();
        var expectedBonus = attackPowerBooster.GetAttackPowerBonus();
        Debug.Log($"铁剑攻击力加成: {expectedBonus}");

        // 将玩家移动到道具位置来触发道具
        var playerGridComponent = player.GetBehaviorComponent<GridObjectComponent>();
        playerGridComponent.MoveTo(new Vector2Int(1, 0));

        // 等待一帧让事件处理完成
        yield return null;

        // 验证玩家攻击力是否增加
        var finalAttackPower = attackPowerComponent.GetAttackPower();
        Debug.Log($"玩家最终攻击力: {finalAttackPower}");
        
        Assert.AreEqual(initialAttackPower + expectedBonus, finalAttackPower, 
            $"玩家攻击力应该增加 {expectedBonus} 点，从 {initialAttackPower} 增加到 {finalAttackPower}");
    }
    
    [UnityTest]
    public IEnumerator IronSwordProp_ShouldUpdateAttackPowerBonus_WhenDamageChanged()
    {
        // 等待一帧让Awake方法执行
        yield return null;

        // 获取初始攻击力加成
        var attackPowerBooster = ironSwordProp.GetBehaviorComponent<AttackPowerBoosterComponent>();
        var initialBonus = attackPowerBooster.GetAttackPowerBonus();

        // 修改铁剑伤害值
        var newDamage = initialBonus + 5;
        ironSwordProp.SetDamage(newDamage);
        yield return null;

        // 验证攻击力加成已更新
        var updatedBonus = attackPowerBooster.GetAttackPowerBonus();
        Assert.AreEqual(newDamage, updatedBonus, 
            $"攻击力加成应该更新为 {newDamage}，实际为 {updatedBonus}");
    }

    [UnityTest]
    public IEnumerator IronSwordProp_ShouldWorkWithMultipleProps()
    {
        // 等待一帧让Awake方法执行
        yield return null;

        // 创建第二个铁剑道具
        var propTypeId = TypeId.Create<PropTypeId>("IronSword");
        var secondIronSword = (IronSwordProp)PropController.Instance.PlaceProp(new Vector2Int(0, 1), propTypeId);
        
        // 获取玩家初始攻击力
        var attackPowerComponent = player.GetBehaviorComponent<AttackPowerComponent>();
        var initialAttackPower = attackPowerComponent.GetAttackPower();

        // 移动到第一个铁剑位置触发
        var playerGridComponent = player.GetBehaviorComponent<GridObjectComponent>();
        playerGridComponent.MoveTo(new Vector2Int(1, 0));
        yield return null;

        var attackPowerAfterFirst = attackPowerComponent.GetAttackPower();
        var firstBonus = ironSwordProp.GetDamage();

        // 移动到第二个铁剑位置触发
        playerGridComponent.MoveTo(new Vector2Int(0, 1));
        yield return null;

        var finalAttackPower = attackPowerComponent.GetAttackPower();
        var secondBonus = secondIronSword.GetDamage();

        // 验证攻击力是两次加成的总和
        var expectedTotal = initialAttackPower + firstBonus + secondBonus;
        Assert.AreEqual(expectedTotal, finalAttackPower, 
            $"最终攻击力应该是初始值 + 第一个加成({firstBonus}) + 第二个加成({secondBonus}) = {expectedTotal}");
    }
}
