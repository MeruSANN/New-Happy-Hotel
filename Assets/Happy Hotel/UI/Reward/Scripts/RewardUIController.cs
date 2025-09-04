using System.Collections.Generic;
using System.Linq;
using HappyHotel.GameManager;
using HappyHotel.Reward;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HappyHotel.UI.Reward
{
    // 总奖励UI控制脚本，管理所有奖励道具的显示
    public class RewardUIController : MonoBehaviour
    {
        [Header("UI组件引用")] [SerializeField] private Transform rewardItemContainer; // 奖励道具容器

        [SerializeField] private RewardItemDisplayController rewardItemPrefab; // 奖励道具显示预制体
        [SerializeField] private Button confirmButton; // 确认按钮

        [Header("按钮文字配置")] [SerializeField] private string giveUpButtonText = "放弃"; // 有未获取奖励时的按钮文字

        [SerializeField] private string nextLevelButtonText = "下一关"; // 无未获取奖励时的按钮文字

        // 当前显示的奖励道具UI列表
        private readonly List<RewardItemDisplayController> rewardItemDisplays = new();

        // 奖励控制器引用
        private RewardClaimController rewardClaimController;

        private void Awake()
        {
            // 绑定确认按钮事件
            if (confirmButton != null) confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        }

        private void Start()
        {
            // 获取管理器实例
            rewardClaimController = RewardClaimController.Instance;

            // 订阅奖励事件
            if (rewardClaimController != null)
            {
                rewardClaimController.onRewardItemClaimed += OnRewardItemClaimed;
                rewardClaimController.onRewardRefreshed += OnRewardRefreshed;
            }

            // LevelCompletionManager会自动查找并设置RewardUIController

            // 初始化显示
            RefreshRewardUI();

            // 确保按钮文字正确
            UpdateConfirmButtonText();
        }

        private void OnEnable()
        {
            if (!rewardClaimController) rewardClaimController = RewardClaimController.Instance;

            RefreshRewardUI();
        }

        private void OnDestroy()
        {
            // 取消订阅奖励事件
            if (rewardClaimController != null)
            {
                rewardClaimController.onRewardItemClaimed -= OnRewardItemClaimed;
                rewardClaimController.onRewardRefreshed -= OnRewardRefreshed;
            }

            // 清理奖励道具UI的事件绑定
            foreach (var display in rewardItemDisplays)
                if (display != null)
                    display.OnClaimClicked -= OnItemClaimClicked;

            // 清理确认按钮事件绑定
            if (confirmButton != null) confirmButton.onClick.RemoveListener(OnConfirmButtonClicked);
        }

        // 刷新奖励UI显示
        private void RefreshRewardUI()
        {
            if (!rewardClaimController)
            {
                Debug.LogWarning("RewardController未初始化，无法刷新奖励UI");
                return;
            }

            // 清理现有的UI
            ClearRewardItemDisplays();

            // 获取当前奖励中的所有道具
            var rewardItems = rewardClaimController.GetAllRewardItems();

            // 为每个道具创建UI显示
            foreach (var rewardItem in rewardItems) CreateRewardItemDisplay(rewardItem);

            // 更新确认按钮文字
            UpdateConfirmButtonText();

            Debug.Log($"奖励UI已刷新，显示 {rewardItemDisplays.Count} 个道具");
        }

        // 创建单个奖励道具的UI显示
        private void CreateRewardItemDisplay(RewardItemBase rewardItem)
        {
            if (rewardItemPrefab == null || rewardItemContainer == null)
            {
                Debug.LogError("奖励道具预制体或容器未设置");
                return;
            }

            // 实例化预制体
            var displayInstance = Instantiate(rewardItemPrefab, rewardItemContainer);

            // 设置要显示的奖励道具
            displayInstance.SetRewardItem(rewardItem);

            // 绑定获取事件
            displayInstance.OnClaimClicked += OnItemClaimClicked;

            // 添加到列表中
            rewardItemDisplays.Add(displayInstance);
        }

        // 清理所有奖励道具UI显示
        private void ClearRewardItemDisplays()
        {
            foreach (var display in rewardItemDisplays)
                if (display != null)
                {
                    // 解绑事件
                    display.OnClaimClicked -= OnItemClaimClicked;

                    // 销毁GameObject
                    Destroy(display.gameObject);
                }

            rewardItemDisplays.Clear();
        }

        // 处理道具获取点击事件
        private void OnItemClaimClicked(RewardItemBase rewardItem)
        {
            if (rewardItem == null || rewardClaimController == null)
                return;

            // 执行获取
            rewardClaimController.ClaimRewardItem(rewardItem);

            Debug.Log($"已获取奖励道具: {rewardItem.ItemName}");
        }

        // 处理奖励道具被获取事件
        private void OnRewardItemClaimed(RewardItemBase rewardItem)
        {
            if (rewardItem != null)
            {
                // 移除对应的UI显示
                RemoveRewardItemUI(rewardItem);

                // 更新确认按钮文字
                UpdateConfirmButtonText();

                Debug.Log($"UI已移除奖励道具: {rewardItem.ItemName}");
            }
        }

        // 处理奖励刷新事件
        private void OnRewardRefreshed()
        {
            Debug.Log("奖励已刷新，更新UI显示");
            RefreshRewardUI();
        }

        // 获取所有按钮点击处理
        private void OnClaimAllButtonClicked()
        {
            if (rewardClaimController == null)
            {
                Debug.LogWarning("奖励控制器未初始化，无法获取所有奖励");
                return;
            }

            var rewardItems = rewardClaimController.GetAllRewardItems();
            if (rewardItems.Count == 0)
            {
                Debug.Log("没有可获取的奖励道具");
                return;
            }

            Debug.Log($"开始获取所有奖励道具，共 {rewardItems.Count} 个");

            // 获取所有奖励道具
            foreach (var rewardItem in rewardItems.ToList()) rewardClaimController.ClaimRewardItem(rewardItem);
        }

        // 添加奖励道具UI
        public void AddRewardItemUI(RewardItemBase rewardItem)
        {
            if (rewardItem == null)
                return;

            // 检查是否已经存在
            if (HasRewardItemUI(rewardItem))
            {
                Debug.LogWarning($"奖励道具UI已存在: {rewardItem.ItemName}");
                return;
            }

            // 创建新的UI显示
            CreateRewardItemDisplay(rewardItem);

            // 更新确认按钮文字
            UpdateConfirmButtonText();

            Debug.Log($"已添加奖励道具UI: {rewardItem.ItemName}");
        }

        // 移除奖励道具UI
        public void RemoveRewardItemUI(RewardItemBase rewardItem)
        {
            if (rewardItem == null)
                return;

            // 查找对应的UI显示
            var displayToRemove = rewardItemDisplays.FirstOrDefault(display =>
                display != null && display.GetCurrentRewardItem() == rewardItem);

            if (displayToRemove != null)
            {
                // 解绑事件
                displayToRemove.OnClaimClicked -= OnItemClaimClicked;

                // 从列表中移除
                rewardItemDisplays.Remove(displayToRemove);

                // 销毁GameObject
                Destroy(displayToRemove.gameObject);

                // 更新确认按钮文字
                UpdateConfirmButtonText();

                Debug.Log($"已移除奖励道具UI: {rewardItem.ItemName}");
            }
        }

        // 获取显示数量
        public int GetDisplayCount()
        {
            rewardItemDisplays.RemoveAll(display => display == null);
            return rewardItemDisplays.Count;
        }

        // 检查是否已有奖励道具UI
        public bool HasRewardItemUI(RewardItemBase rewardItem)
        {
            return rewardItemDisplays.Any(display =>
                display != null && display.GetCurrentRewardItem() == rewardItem);
        }

        // 获取奖励道具显示控制器
        public RewardItemDisplayController GetRewardItemDisplay(RewardItemBase rewardItem)
        {
            return rewardItemDisplays.FirstOrDefault(display =>
                display != null && display.GetCurrentRewardItem() == rewardItem);
        }

        // 设置奖励道具容器
        public void SetRewardItemContainer(Transform container)
        {
            rewardItemContainer = container;
        }

        // 设置奖励道具预制体
        public void SetRewardItemPrefab(RewardItemDisplayController prefab)
        {
            rewardItemPrefab = prefab;
        }

        // 清空所有奖励（用于测试）
        public void ClearAllRewards()
        {
            if (rewardClaimController != null) rewardClaimController.ClearRewards();
        }

        // 确认按钮点击处理
        private void OnConfirmButtonClicked()
        {
            Debug.Log("奖励面板确认按钮被点击");

            // 通知LevelCompletionManager关闭奖励面板
            if (LevelCompletionManager.Instance != null) LevelCompletionManager.Instance.OnRewardConfirmButtonClicked();
        }

        // 设置确认按钮
        public void SetConfirmButton(Button button)
        {
            // 清理旧的绑定
            if (confirmButton != null) confirmButton.onClick.RemoveListener(OnConfirmButtonClicked);

            confirmButton = button;

            // 绑定新的事件
            if (confirmButton != null) confirmButton.onClick.AddListener(OnConfirmButtonClicked);

            // 更新按钮文字
            UpdateConfirmButtonText();
        }

        // 更新确认按钮文字
        private void UpdateConfirmButtonText()
        {
            if (confirmButton == null)
                return;

            // 获取按钮的文字组件
            var buttonText = confirmButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText == null)
            {
                // 尝试获取普通的Text组件
                var legacyText = confirmButton.GetComponentInChildren<Text>();
                if (legacyText != null)
                {
                    // 检查是否还有未获取的奖励
                    var hasUnclaimedRewards = HasUnclaimedRewards();
                    legacyText.text = hasUnclaimedRewards ? giveUpButtonText : nextLevelButtonText;

                    Debug.Log($"确认按钮文字已更新为: {legacyText.text} (有未获取奖励: {hasUnclaimedRewards})");
                }
                else
                {
                    Debug.LogWarning("确认按钮没有找到文字组件 (TextMeshProUGUI 或 Text)");
                }

                return;
            }

            // 检查是否还有未获取的奖励
            var hasUnclaimed = HasUnclaimedRewards();
            buttonText.text = hasUnclaimed ? giveUpButtonText : nextLevelButtonText;

            Debug.Log($"确认按钮文字已更新为: {buttonText.text} (有未获取奖励: {hasUnclaimed})");
        }

        // 检查是否还有未获取的奖励
        private bool HasUnclaimedRewards()
        {
            if (rewardClaimController == null)
                return false;

            var rewardItems = rewardClaimController.GetAllRewardItems();
            return rewardItems.Count > 0;
        }

        // 设置按钮文字配置
        public void SetButtonTextConfig(string giveUpText, string nextLevelText)
        {
            giveUpButtonText = giveUpText;
            nextLevelButtonText = nextLevelText;

            // 立即更新按钮文字
            UpdateConfirmButtonText();
        }

        // 获取当前按钮文字配置
        public (string giveUpText, string nextLevelText) GetButtonTextConfig()
        {
            return (giveUpButtonText, nextLevelButtonText);
        }
    }
}