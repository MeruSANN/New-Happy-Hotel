using System.Collections;
using System.Collections.Generic;
using HappyHotel.Character;
using HappyHotel.Core.Rarity;
using HappyHotel.Core.Registry;
using HappyHotel.Core.Singleton;
using HappyHotel.Shop;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Tilemaps;
using CharacterController = HappyHotel.Character.CharacterController;

public class ShopTest
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
    public IEnumerator TestShopManagerInitialization()
    {
        // 等待一帧让Start方法执行
        yield return null;

        // 验证ShopItemManager是否正确初始化
        Assert.IsNotNull(ShopItemManager.Instance, "ShopItemManager应该被正确初始化");
        Assert.IsTrue(ShopItemManager.Instance.IsInitialized, "ShopItemManager应该处于已初始化状态");

        // 验证ShopController和ShopMoneyManager初始化
        Assert.IsNotNull(ShopController.Instance, "ShopController应该被正确初始化");
        Assert.IsNotNull(ShopMoneyManager.Instance, "ShopMoneyManager应该被正确初始化");

        // 验证初始状态
        Assert.AreEqual(1000, ShopMoneyManager.Instance.CurrentMoney, "初始金币应该为1000");
        Assert.AreEqual(0, ShopController.Instance.GetAllShopItems().Count, "初始商店应该为空");
    }

    [UnityTest]
    public IEnumerator TestMoneyManagement()
    {
        // 等待一帧让Start方法执行
        yield return null;

        var moneyManager = ShopMoneyManager.Instance;

        // 测试设置金币
        moneyManager.SetCurrentMoney(500);
        Assert.AreEqual(500, moneyManager.CurrentMoney, "设置金币后应该为500");

        // 测试增加金币
        moneyManager.AddMoney(200);
        Assert.AreEqual(700, moneyManager.CurrentMoney, "增加200金币后应该为700");

        // 测试花费金币
        var spendResult = moneyManager.SpendMoney(300);
        Assert.IsTrue(spendResult, "应该能够花费300金币");
        Assert.AreEqual(400, moneyManager.CurrentMoney, "花费300金币后应该剩余400");

        // 测试花费超过拥有的金币
        var overspendResult = moneyManager.SpendMoney(500);
        Assert.IsFalse(overspendResult, "不应该能够花费超过拥有的金币");
        Assert.AreEqual(400, moneyManager.CurrentMoney, "金币数量不应该改变");

        // 测试负数金币设置
        moneyManager.SetCurrentMoney(-100);
        Assert.AreEqual(0, moneyManager.CurrentMoney, "负数金币应该被设置为0");
    }

    [UnityTest]
    public IEnumerator TestShopRefresh()
    {
        // 等待一帧让Start方法执行
        yield return null;

        var shopController = ShopController.Instance;

        // 刷新商店
        shopController.RefreshShop();

        // 等待一帧让刷新完成
        yield return null;

        var shopItems = shopController.GetAllShopItems();

        // 验证商店刷新结果
        Assert.IsTrue(shopItems.Count > 0, "刷新后商店应该有道具");
        Assert.IsTrue(shopItems.Count <= shopController.TotalMaxShopItems, "商店道具数量不应该超过上限");

        Debug.Log($"商店刷新后有 {shopItems.Count} 个道具");

        // 验证道具不重复（如果有足够的道具类型）
        var itemNames = new HashSet<TypeId>();
        foreach (var item in shopItems)
        {
            Assert.IsFalse(itemNames.Contains(item.TypeId), $"道具 {item.TypeId} 重复出现");
            itemNames.Add(item.TypeId);
            Debug.Log($"商店道具: {item.TypeId}, 价格: {item.Price}, 稀有度: {item.Rarity}");
        }

        // 测试多次刷新
        shopController.RefreshShop();
        yield return null;

        var newShopItems = shopController.GetAllShopItems();
        Assert.IsTrue(newShopItems.Count > 0, "再次刷新后商店应该仍有道具");
    }

    [UnityTest]
    public IEnumerator TestShopClear()
    {
        // 等待一帧让Start方法执行
        yield return null;

        var shopController = ShopController.Instance;

        // 刷新商店添加一些道具
        shopController.RefreshShop();
        yield return null;

        Assert.IsTrue(shopController.GetAllShopItems().Count > 0, "刷新后商店应该有道具");

        // 清空商店
        shopController.ClearShop();
        Assert.AreEqual(0, shopController.GetAllShopItems().Count, "清空后商店应该为空");

        Debug.Log("商店清空测试通过");
    }

    [UnityTest]
    public IEnumerator TestRarityDistribution()
    {
        // 等待一帧让Start方法执行
        yield return null;

        var shopController = ShopController.Instance;

        // 多次刷新商店，统计稀有度分布
        var rarityCount = new Dictionary<Rarity, int>();
        var refreshCount = 10;

        for (var i = 0; i < refreshCount; i++)
        {
            shopController.RefreshShop();
            yield return null;

            var shopItems = shopController.GetAllShopItems();
            foreach (var item in shopItems)
            {
                if (!rarityCount.ContainsKey(item.Rarity)) rarityCount[item.Rarity] = 0;

                rarityCount[item.Rarity]++;
            }
        }

        // 输出稀有度分布统计
        Debug.Log("稀有度分布统计（共" + refreshCount + "次刷新）:");
        foreach (var kvp in rarityCount) Debug.Log($"{kvp.Key}: {kvp.Value} 次");

        // 验证普通道具出现次数最多（基于权重配置）
        if (rarityCount.ContainsKey(Rarity.Common) && rarityCount.ContainsKey(Rarity.Legendary))
            Assert.IsTrue(rarityCount[Rarity.Common] >= rarityCount[Rarity.Legendary],
                "普通道具出现次数应该大于等于传说道具");
    }
}