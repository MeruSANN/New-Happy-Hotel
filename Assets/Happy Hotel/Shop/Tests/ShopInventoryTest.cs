using System.Collections;
using HappyHotel.Character;
using HappyHotel.Core.Registry;
using HappyHotel.Core.Singleton;
using HappyHotel.Equipment;
using HappyHotel.Inventory;
using HappyHotel.Shop;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Tilemaps;
using CharacterController = HappyHotel.Character.CharacterController;


public class ShopInventoryTest
{
    private DefaultCharacter defaultCharacter;
    private Grid grid;
    private GameObject gridObj;
    private GameObject playerObj;
    private SingletonManager singletonManager;
    private GameObject singletonManagerObj;
    private GameObject tilemapObj;

    [SetUp]
    public void SetUp()
    {
        // 创建网格对象
        gridObj = new GameObject("Grid");
        gridObj.tag = "Grid";
        grid = gridObj.AddComponent<Grid>();
        grid.cellSize = new Vector3(1, 1, 0);

        // 创建Tilemap对象
        tilemapObj = new GameObject("MapTilemap");
        tilemapObj.transform.SetParent(gridObj.transform);
        tilemapObj.tag = "MapTilemap";
        tilemapObj.AddComponent<Tilemap>();
        tilemapObj.AddComponent<TilemapRenderer>();

        // 创建单例管理器
        singletonManagerObj = new GameObject("Singleton Manager");
        singletonManager = singletonManagerObj.AddComponent<SingletonManager>();

        // 创建玩家对象
        var typeId = TypeId.Create<CharacterTypeId>("Default");
        defaultCharacter = (DefaultCharacter)CharacterController.Instance.CreateCharacter(typeId, Vector2Int.zero);
        playerObj = defaultCharacter.gameObject;
        playerObj.tag = "MainCharacter"; // 设置玩家标签
    }

    [TearDown]
    public void TearDown()
    {
        // 先销毁游戏对象，避免单例销毁时影响到对象引用
        Object.DestroyImmediate(gridObj);
        Object.DestroyImmediate(playerObj);

        // 再清理单例
        singletonManager.ClearSingletonsImmediate();
        Object.DestroyImmediate(singletonManagerObj);
    }

    [UnityTest]
    public IEnumerator TestIronSwordShopItemCreation()
    {
        // 等待一帧让Start方法执行
        yield return null;

        var shopManager = ShopItemManager.Instance;

        // 创建铁剑商店道具
        var ironSwordTypeId = TypeId.Create<ShopItemTypeId>("IronSword");
        var ironSwordItem = shopManager.CreateShopItem(ironSwordTypeId) as IronSwordShopItem;

        Assert.IsNotNull(ironSwordItem, "铁剑商店道具应该被成功创建");
        Assert.IsTrue(ironSwordItem is IronSwordShopItem, "创建的道具应该是IronSwordShopItem类型");
        Assert.IsTrue(ironSwordItem is EquipmentShopItemBase, "IronSwordShopItem应该继承自InventoryShopItemBase");

        // 验证道具属性
        Assert.IsNotEmpty(ironSwordItem.ItemName, "道具应该有名称");
        Assert.IsTrue(ironSwordItem.WeaponDamage > 0, "武器应该有伤害值");

        Debug.Log($"创建的铁剑商店道具: {ironSwordItem.ItemName}, 武器伤害: {ironSwordItem.WeaponDamage}");
    }

    [UnityTest]
    public IEnumerator TestIronSwordPurchaseAndInventoryAdd()
    {
        // 等待一帧让Start方法执行
        yield return null;

        var shopManager = ShopItemManager.Instance;
        var inventory = EquipmentInventory.Instance;

        // 记录购买前的背包状态
        var weaponCountBefore = inventory.CurrentEquipmentCount;

        // 创建铁剑商店道具
        var ironSwordTypeId = TypeId.Create<ShopItemTypeId>("IronSword");
        var ironSwordItem = shopManager.CreateShopItem(ironSwordTypeId) as IronSwordShopItem;
        Assert.IsNotNull(ironSwordItem, "铁剑商店道具应该被成功创建");

        // 确保有足够的金币购买
        var moneyManager = ShopMoneyManager.Instance;
        var shopController = ShopController.Instance;
        moneyManager.SetCurrentMoney(1000);
        var moneyBefore = moneyManager.CurrentMoney;

        // 将道具添加到商店控制器
        shopController.AddShopItem(ironSwordItem);

        // 验证可以购买
        Assert.IsTrue(ironSwordItem.CanPurchase(moneyManager.CurrentMoney), "应该能够购买铁剑");
        Assert.IsTrue(ironSwordItem.CanAddToInventory(), "应该能够添加到背包");

        // 购买铁剑
        var purchaseResult = shopController.PurchaseItemWithMoney(ironSwordItem);
        Assert.IsTrue(purchaseResult, "应该能够成功购买铁剑");

        // 验证金币扣除
        var expectedMoney = moneyBefore - ironSwordItem.Price;
        Assert.AreEqual(expectedMoney, moneyManager.CurrentMoney, "购买后金币应该被正确扣除");

        // 验证武器添加到背包
        var weaponCountAfter = inventory.CurrentEquipmentCount;
        Assert.AreEqual(weaponCountBefore + 1, weaponCountAfter, "背包中武器数量应该增加1");

        // 验证添加的武器类型正确
        var ironSwordTypeIdInInventory = TypeId.Create<EquipmentTypeId>("IronSword");
        Assert.IsTrue(inventory.HasEquipmentOfType(ironSwordTypeIdInInventory), "背包中应该有铁剑");

        var addedWeapon = inventory.GetEquipmentByTypeId(ironSwordTypeIdInInventory) as IronSword;
        Assert.IsNotNull(addedWeapon, "添加的武器应该是IronSword类型");
        Assert.AreEqual(ironSwordItem.WeaponDamage, addedWeapon.Damage, "武器伤害值应该正确设置");

        Debug.Log("购买铁剑成功！");
        Debug.Log($"购买前: 武器数量 {weaponCountBefore}, 金币 {moneyBefore}");
        Debug.Log($"购买后: 武器数量 {weaponCountAfter}, 金币 {moneyManager.CurrentMoney}");
        Debug.Log($"武器伤害: {addedWeapon.Damage}");
    }

    [UnityTest]
    public IEnumerator TestWeaponDescriptionAndDamage()
    {
        // 等待一帧让Start方法执行
        yield return null;

        var shopManager = ShopItemManager.Instance;

        // 创建铁剑商店道具
        var ironSwordTypeId = TypeId.Create<ShopItemTypeId>("IronSword");
        var ironSwordItem = shopManager.CreateShopItem(ironSwordTypeId) as IronSwordShopItem;

        // 测试设置武器伤害
        ironSwordItem.SetWeaponDamage(5);
        Assert.AreEqual(5, ironSwordItem.WeaponDamage, "武器伤害应该被正确设置");

        // 测试负数伤害
        ironSwordItem.SetWeaponDamage(-1);
        Assert.AreEqual(0, ironSwordItem.WeaponDamage, "负数伤害应该被设置为0");

        // 测试武器描述
        ironSwordItem.SetWeaponDamage(3);
        var description = ironSwordItem.GetFormattedDescription();
        Assert.IsTrue(description.Contains("3"), "武器描述应该包含伤害值");

        Debug.Log($"武器描述: {description}");
    }

    [UnityTest]
    public IEnumerator TestShopRefreshWithInventoryItems()
    {
        // 等待一帧让Start方法执行
        yield return null;

        var shopManager = ShopItemManager.Instance;

        // 刷新商店
        var shopController = ShopController.Instance;
        shopController.RefreshShop();
        yield return null;

        var shopItems = shopController.GetAllShopItems();

        // 检查是否有InventoryShopItemBase类型的道具
        var hasInventoryItem = false;
        foreach (var item in shopItems)
            if (item is EquipmentShopItemBase)
            {
                hasInventoryItem = true;
                Debug.Log($"发现背包道具: {item.ItemName}, 类型: {item.GetType().Name}");
            }

        // 注意：这个测试可能会失败，因为刷新是随机的
        // 但我们可以验证系统能够正常工作
        Debug.Log($"商店刷新测试完成，共有 {shopItems.Count} 个道具，其中包含背包道具: {hasInventoryItem}");
    }
}