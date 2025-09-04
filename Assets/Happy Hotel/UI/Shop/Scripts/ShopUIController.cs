using System.Collections.Generic;
using System.Linq;
using HappyHotel.Core.Scene;
using HappyHotel.Shop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HappyHotel.UI.Shop
{
    // 总商店UI控制脚本，管理所有商店道具的显示
    public class ShopUIController : MonoBehaviour
    {
        // 装备道具容器
        [Header("显示控制器列表")] [SerializeField]
        private List<ShopCardDisplayController> cardDisplayControllers; // 卡牌显示控制器列表

        [SerializeField] private List<ShopEquipmentDisplayController> equipmentDisplayControllers; // 装备显示控制器列表

        [Header("按钮")] [SerializeField] private Button exitShopButton; // 退出商店按钮

        [Header("金币显示")] [SerializeField] private TextMeshProUGUI moneyText; // 金币显示文本

        private ShopMoneyManager moneyManager;

        // 商店控制器引用
        private ShopController shopController;

        private void Awake()
        {
            // 绑定退出商店按钮事件
            if (exitShopButton != null) exitShopButton.onClick.AddListener(OnExitShopButtonClicked);
        }

        private void Start()
        {
            // 获取管理器实例
            shopController = ShopController.Instance;
            moneyManager = ShopMoneyManager.Instance;

            // 订阅商店道具移除事件
            if (shopController != null)
            {
                shopController.onShopItemRemoved += OnShopItemRemoved;
                shopController.onShopRefreshed += OnShopRefreshed;
            }

            // 绑定所有显示控制器的事件
            BindDisplayControllerEvents();

            // 初始化显示
            RefreshShopUI();
            UpdateMoneyDisplay();
        }

        private void Update()
        {
            // 定期更新金币显示
            UpdateMoneyDisplay();
        }

        private void OnDestroy()
        {
            // 清理退出商店按钮事件绑定
            if (exitShopButton != null) exitShopButton.onClick.RemoveListener(OnExitShopButtonClicked);

            // 取消订阅商店道具移除事件
            if (shopController != null)
            {
                shopController.onShopItemRemoved -= OnShopItemRemoved;
                shopController.onShopRefreshed -= OnShopRefreshed;
            }

            // 清理所有显示控制器的事件绑定
            UnbindDisplayControllerEvents();
        }

        // 刷新商店UI显示
        public void RefreshShopUI()
        {
            if (shopController == null)
            {
                Debug.LogWarning("ShopController未初始化，无法刷新商店UI");
                return;
            }

            // 清空所有显示控制器
            ClearAllDisplayControllers();

            // 获取当前商店中的所有道具
            var shopItems = shopController.GetAllShopItems();

            // 为每个道具分配显示控制器并重置售罄状态
            AssignShopItemsToControllers(shopItems);

            var totalDisplays = GetActiveDisplayCount();
            Debug.Log($"商店UI已刷新，显示 {totalDisplays} 个道具");
        }

        // 绑定所有显示控制器的事件
        private void BindDisplayControllerEvents()
        {
            // 绑定卡牌显示控制器事件
            if (cardDisplayControllers != null)
                foreach (var controller in cardDisplayControllers)
                    if (controller != null)
                    {
                        controller.onPurchaseClicked += OnItemPurchaseClicked;
                        controller.onItemHoverExit += OnItemHoverExit;
                    }

            // 绑定装备显示控制器事件
            if (equipmentDisplayControllers != null)
                foreach (var controller in equipmentDisplayControllers)
                    if (controller != null)
                    {
                        controller.onPurchaseClicked += OnItemPurchaseClicked;
                        controller.onItemHoverExit += OnItemHoverExit;
                    }
        }

        // 解绑所有显示控制器的事件
        private void UnbindDisplayControllerEvents()
        {
            // 解绑卡牌显示控制器事件
            if (cardDisplayControllers != null)
                foreach (var controller in cardDisplayControllers)
                    if (controller != null)
                    {
                        controller.onPurchaseClicked -= OnItemPurchaseClicked;
                        controller.onItemHoverExit -= OnItemHoverExit;
                    }

            // 解绑装备显示控制器事件
            if (equipmentDisplayControllers != null)
                foreach (var controller in equipmentDisplayControllers)
                    if (controller != null)
                    {
                        controller.onPurchaseClicked -= OnItemPurchaseClicked;
                        controller.onItemHoverExit -= OnItemHoverExit;
                    }
        }

        // 为商店道具分配显示控制器
        private void AssignShopItemsToControllers(List<ShopItemBase> shopItems)
        {
            var cardControllerIndex = 0;
            var equipmentControllerIndex = 0;

            foreach (var shopItem in shopItems)
                if (shopItem is CardShopItemBase cardShopItemBase)
                {
                    // 分配给卡牌显示控制器
                    if (cardDisplayControllers != null && cardControllerIndex < cardDisplayControllers.Count)
                    {
                        var controller = cardDisplayControllers[cardControllerIndex];
                        if (controller != null)
                        {
                            controller.SetShopItem(cardShopItemBase);
                            controller.SetSoldOut(false); // 重置售罄状态
                        }

                        cardControllerIndex++;
                    }
                }
                else if (shopItem is EquipmentShopItemBase)
                {
                    // 分配给装备显示控制器
                    if (equipmentDisplayControllers != null &&
                        equipmentControllerIndex < equipmentDisplayControllers.Count)
                    {
                        var controller = equipmentDisplayControllers[equipmentControllerIndex];
                        if (controller != null)
                        {
                            controller.SetShopItem(shopItem);
                            controller.SetSoldOut(false); // 重置售罄状态
                        }

                        equipmentControllerIndex++;
                    }
                }

            // 清空剩余的控制器
            ClearRemainingControllers(cardControllerIndex, equipmentControllerIndex);
        }

        // 清空剩余的显示控制器
        private void ClearRemainingControllers(int cardControllerIndex, int equipmentControllerIndex)
        {
            // 清空剩余的卡牌控制器
            if (cardDisplayControllers != null)
                for (var i = cardControllerIndex; i < cardDisplayControllers.Count; i++)
                    if (cardDisplayControllers[i] != null)
                        cardDisplayControllers[i].ClearDisplay();

            // 清空剩余的装备控制器
            if (equipmentDisplayControllers != null)
                for (var i = equipmentControllerIndex; i < equipmentDisplayControllers.Count; i++)
                    if (equipmentDisplayControllers[i] != null)
                        equipmentDisplayControllers[i].ClearDisplay();
        }

        // 清空所有显示控制器
        private void ClearAllDisplayControllers()
        {
            // 清空卡牌显示控制器
            if (cardDisplayControllers != null)
                foreach (var controller in cardDisplayControllers)
                    if (controller != null)
                        controller.ClearDisplay();

            // 清空装备显示控制器
            if (equipmentDisplayControllers != null)
                foreach (var controller in equipmentDisplayControllers)
                    if (controller != null)
                        controller.ClearDisplay();
        }

        // 处理道具购买点击事件
        private void OnItemPurchaseClicked(ShopItemBase shopItem)
        {
            if (shopItem == null || shopController == null)
                return;

            // 执行购买
            var purchaseSuccess = shopController.PurchaseItemWithMoney(shopItem);

            if (purchaseSuccess)
            {
                // 购买成功后更新金币显示
                UpdateMoneyDisplay();

                // 将道具设为售罄状态
                SetShopItemSoldOut(shopItem);

                Debug.Log($"成功购买道具: {shopItem.ItemName}");
            }
            else
            {
                Debug.LogWarning($"购买道具失败: {shopItem.ItemName}");
            }
        }

        // 处理商店道具被移除事件
        private void OnShopItemRemoved(ShopItemBase shopItem)
        {
            if (shopItem != null)
            {
                // 将对应的显示控制器设为售罄状态
                SetShopItemSoldOut(shopItem);
                Debug.Log($"道具已设为售罄: {shopItem.ItemName}");
            }
        }

        // 处理商店刷新事件
        private void OnShopRefreshed()
        {
            Debug.Log("收到商店刷新事件，更新UI显示");
            // 刷新商店UI，这会重新分配道具并重置所有售罄状态
            RefreshShopUI();
        }

        // 刷新所有商店道具显示（不重新创建UI，只更新显示内容）
        private void RefreshAllDisplays()
        {
            // 刷新卡牌显示控制器
            if (cardDisplayControllers != null)
                foreach (var controller in cardDisplayControllers)
                    if (controller != null)
                        controller.RefreshDisplay();

            // 刷新装备显示控制器
            if (equipmentDisplayControllers != null)
                foreach (var controller in equipmentDisplayControllers)
                    if (controller != null)
                        controller.RefreshDisplay();
        }

        // 更新金币显示
        private void UpdateMoneyDisplay()
        {
            if (moneyText != null && moneyManager != null) moneyText.text = $"金币: {moneyManager.CurrentMoney}";
        }

        // 设置商店道具为售罄状态
        public void SetShopItemSoldOut(ShopItemBase shopItem)
        {
            if (shopItem == null)
                return;

            // 查找对应的显示控制器并设为售罄状态
            var controller = GetShopItemDisplayController(shopItem);
            if (controller != null)
            {
                if (controller is ShopCardDisplayController cardController)
                    cardController.SetSoldOut(true);
                else if (controller is ShopEquipmentDisplayController equipmentController)
                    equipmentController.SetSoldOut(true);

                Debug.Log($"道具 {shopItem.ItemName} 已设为售罄状态");
            }
        }

        // 获取当前显示的商店道具UI数量
        public int GetDisplayCount()
        {
            return GetActiveDisplayCount();
        }

        // 获取活跃的显示控制器数量
        public int GetActiveDisplayCount()
        {
            var count = 0;

            // 统计卡牌显示控制器
            if (cardDisplayControllers != null)
                foreach (var controller in cardDisplayControllers)
                    if (controller != null && controller.GetCurrentShopItem() != null)
                        count++;

            // 统计装备显示控制器
            if (equipmentDisplayControllers != null)
                foreach (var controller in equipmentDisplayControllers)
                    if (controller != null && controller.GetCurrentShopItem() != null)
                        count++;

            return count;
        }

        // 获取卡片道具UI数量
        public int GetCardDisplayCount()
        {
            var count = 0;
            if (cardDisplayControllers != null)
                foreach (var controller in cardDisplayControllers)
                    if (controller != null && controller.GetCurrentShopItem() != null)
                        count++;

            return count;
        }

        // 获取装备道具UI数量
        public int GetEquipmentDisplayCount()
        {
            var count = 0;
            if (equipmentDisplayControllers != null)
                foreach (var controller in equipmentDisplayControllers)
                    if (controller != null && controller.GetCurrentShopItem() != null)
                        count++;

            return count;
        }

        // 检查是否有指定道具的UI显示
        public bool HasShopItemUI(ShopItemBase shopItem)
        {
            if (shopItem == null)
                return false;

            return GetShopItemDisplayController(shopItem) != null;
        }

        // 获取指定道具的显示控制器
        public MonoBehaviour GetShopItemDisplayController(ShopItemBase shopItem)
        {
            if (shopItem == null)
                return null;

            // 根据道具类型在对应的列表中查找
            if (shopItem is CardShopItemBase)
            {
                if (cardDisplayControllers != null)
                    return cardDisplayControllers.FirstOrDefault(c => c != null && c.GetCurrentShopItem() == shopItem);
            }
            else if (shopItem is EquipmentShopItemBase)
            {
                if (equipmentDisplayControllers != null)
                    return equipmentDisplayControllers.FirstOrDefault(c =>
                        c != null && c.GetCurrentShopItem() == shopItem);
            }

            return null;
        }

        // 设置卡牌显示控制器列表
        public void SetCardDisplayControllers(List<ShopCardDisplayController> controllers)
        {
            cardDisplayControllers = controllers;
        }

        // 设置装备显示控制器列表
        public void SetEquipmentDisplayControllers(List<ShopEquipmentDisplayController> controllers)
        {
            equipmentDisplayControllers = controllers;
        }

        // 处理物品鼠标悬停退出事件
        private void OnItemHoverExit()
        {
            // HideItemDescription();
        }

        // 退出商店按钮点击处理
        private void OnExitShopButtonClicked()
        {
            Debug.Log("点击退出商店按钮");

            // 检查场景转换管理器是否存在
            if (SceneTransitionManager.Instance == null)
            {
                Debug.LogError("SceneTransitionManager不存在，无法退出商店");
                return;
            }

            // 检查是否正在进行场景转换
            if (SceneTransitionManager.Instance.IsTransitioning())
            {
                Debug.LogWarning("场景转换进行中，请稍后再试");
                return;
            }

            // 检查当前是否在商店场景
            var currentScene = SceneTransitionManager.Instance.GetCurrentSceneName();
            if (currentScene != "ShopScene")
            {
                Debug.LogWarning($"当前不在商店场景 (当前场景: {currentScene})，无法执行退出商店操作");
                return;
            }

            // 退出商店，返回游戏界面并进入下一关
            SceneTransitionManager.Instance.ExitShop();
        }

        // 设置退出商店按钮
        public void SetExitShopButton(Button button)
        {
            // 清理旧按钮事件
            if (exitShopButton != null) exitShopButton.onClick.RemoveListener(OnExitShopButtonClicked);

            // 设置新按钮
            exitShopButton = button;

            // 绑定新按钮事件
            if (exitShopButton != null) exitShopButton.onClick.AddListener(OnExitShopButtonClicked);
        }
    }
}