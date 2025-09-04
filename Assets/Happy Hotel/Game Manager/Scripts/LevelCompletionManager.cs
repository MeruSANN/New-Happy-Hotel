using System;
using System.Collections;
using HappyHotel.Buff.Components;
using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.Scene;
using HappyHotel.Core.Singleton;
using HappyHotel.Reward;
using HappyHotel.UI;
using HappyHotel.UI.Reward;
using UnityEngine;

namespace HappyHotel.GameManager
{
    // 关卡完成管理器，负责处理关卡完成后的逻辑
    [ManagedSingleton(SceneLoadMode.Exclude, "ShopScene", "MainMenu", "MapEditScene")]
    public class LevelCompletionManager : SingletonBase<LevelCompletionManager>
    {
        // 当前状态
        private bool isShowingRewards;
        private int pendingNextLevelBranchIndex;

        // 奖励UI控制器引用
        private RewardUIController rewardUIController;

        protected override void OnDestroy()
        {
            // 取消订阅奖励事件
            if (RewardClaimController.Instance != null)
                RewardClaimController.Instance.onRewardItemClaimed -= OnRewardItemClaimed;
        }

        // 关卡完成事件
        public static event Action<string> onLevelCompleted;
        public static event Action<string> onEnteringShop;
        public static event Action<string, int> onEnteringNextLevel;
        public static event System.Action onRewardPanelShown;
        public static event System.Action onRewardPanelHidden;

        protected override void OnSingletonAwake()
        {
            base.OnSingletonAwake();

            // 延迟查找奖励UI控制器，等待所有对象初始化完成
            StartCoroutine(InitializeRewardUIController());

            // 订阅奖励事件
            if (RewardClaimController.Instance != null)
                RewardClaimController.Instance.onRewardItemClaimed += OnRewardItemClaimed;

            Debug.Log("LevelCompletionManager 初始化完成");
        }

        // 延迟初始化奖励UI控制器
        private IEnumerator InitializeRewardUIController()
        {
            // 等待一帧，确保所有对象都已初始化
            yield return null;

            // 查找奖励UI控制器（包括非活跃的对象）
            rewardUIController = FindObjectOfType<RewardUIController>(true);
            if (rewardUIController == null)
                Debug.LogWarning("LevelCompletionManager: 未找到RewardUIController");
            else
                Debug.Log("LevelCompletionManager: 成功找到RewardUIController");
        }

        // 处理关卡完成逻辑
        public void CompleteLevel(int nextLevelBranchIndex = 0)
        {
            var currentLevelName = GetCurrentLevelName();

            if (string.IsNullOrEmpty(currentLevelName))
            {
                Debug.LogError("LevelCompletionManager: 无法获取当前关卡名称");
                return;
            }

            // 通知主角色卸载自己身上的所有Buff
            try
            {
                var mainCharacter = GameObject.FindGameObjectWithTag("MainCharacter");
                if (mainCharacter)
                {
                    var behaviorContainer = mainCharacter.GetComponent<BehaviorComponentContainer>();
                    if (behaviorContainer)
                    {
                        var buffContainer = behaviorContainer.GetBehaviorComponent<BuffContainer>();
                        if (buffContainer != null)
                            buffContainer.ClearAllBuffs();
                        else Debug.LogWarning("LevelCompletionManager: 未找到 BuffContainer");
                    }
                    else
                    {
                        Debug.LogWarning("LevelCompletionManager: 未找到 BehaviorComponentContainer");
                    }
                }
                else
                {
                    Debug.LogWarning("LevelCompletionManager: 未找到带有 MainCharacter 标签的对象");
                }
            }
            catch
            {
            }

            Debug.Log($"关卡完成: {currentLevelName}");
            onLevelCompleted?.Invoke(currentLevelName);

            // 在显示奖励和流转前，清理本关临时新增的卡，保留奖励后续添加
            if (Inventory.CardInventory.Instance != null)
                Inventory.CardInventory.Instance.CleanupToBaseline();

            // 检查是否为最终关卡
            var isFinalLevel = false;
            if (LevelStateManager.Instance)
            {
                var data = LevelStateManager.Instance.GetCurrentLevelData();
                if (data != null) isFinalLevel = data.isFinalLevel;
            }

            if (isFinalLevel)
            {
                Debug.Log("当前为最终关卡，直接显示胜利结算面板，跳过奖励");
                ShowGameResultAndEnd(true);
                return;
            }

            // 处理金币奖励发放
            HandleCoinReward();

            // 保存下一关分支索引
            pendingNextLevelBranchIndex = nextLevelBranchIndex;

            // 显示奖励面板
            ShowRewardPanel();
        }

        // 显示奖励面板
        private void ShowRewardPanel()
        {
            // 检查配置是否启用奖励面板
            if (!ShouldShowRewardPanel())
            {
                Debug.Log("奖励面板已在配置中禁用，直接继续关卡流程");
                ContinueLevelFlow();
                return;
            }

            Debug.Log("显示奖励面板");

            // 先刷新奖励并检查是否有奖励，若无则直接跳过奖励面板
            if (RewardClaimController.Instance)
            {
                RewardClaimController.Instance.RefreshRewardsForLevelCompletion();
                var rewardCount = RewardClaimController.Instance.GetRewardItemCount();
                if (rewardCount <= 0)
                {
                    Debug.Log("当前无任何奖励项，跳过奖励面板，直接继续关卡流程");
                    ContinueLevelFlow();
                    return;
                }
            }
            else
            {
                // 无法获取奖励控制器时也跳过面板，避免显示空面板
                Debug.LogWarning("LevelCompletionManager: RewardClaimController不存在，跳过奖励面板");
                ContinueLevelFlow();
                return;
            }

            // 如果还没有找到RewardUIController，再次尝试查找（仅当确有奖励需要展示时）
            if (rewardUIController == null)
            {
                rewardUIController = FindObjectOfType<RewardUIController>(true);
                if (rewardUIController == null)
                {
                    Debug.LogError("LevelCompletionManager: 无法找到RewardUIController，无法显示奖励面板，改为直接继续关卡流程");
                    ContinueLevelFlow();
                    return;
                }

                Debug.Log("LevelCompletionManager: 在显示奖励时成功找到RewardUIController");
            }

            // 设置游戏状态为奖励状态（阻止玩家操作）
            if (GameManager.Instance) GameManager.Instance.SetGameState(GameManager.GameState.Reward);

            // 激活奖励UI面板
            if (rewardUIController && rewardUIController.gameObject) rewardUIController.gameObject.SetActive(true);

            isShowingRewards = true;
            onRewardPanelShown?.Invoke();
        }

        // 隐藏奖励面板
        private void HideRewardPanel()
        {
            Debug.Log("隐藏奖励面板");

            // 清理剩余的奖励（避免资源浪费）
            if (RewardClaimController.Instance)
            {
                var remainingRewards = RewardClaimController.Instance.GetRewardItemCount();
                if (remainingRewards > 0)
                {
                    Debug.Log($"清理剩余的 {remainingRewards} 个奖励");
                    RewardClaimController.Instance.ClearRewardsSilently();
                }
            }

            // 隐藏奖励UI面板
            if (rewardUIController && rewardUIController.gameObject) rewardUIController.gameObject.SetActive(false);

            isShowingRewards = false;
            onRewardPanelHidden?.Invoke();

            // 继续关卡流程
            ContinueLevelFlow();
        }

        // 继续关卡流程（奖励获取完成后）
        private void ContinueLevelFlow()
        {
            // 恢复游戏状态为待机
            if (GameManager.Instance) GameManager.Instance.SetGameState(GameManager.GameState.Idle);

            // 检查当前地图是否需要进入商店
            var shouldEnterShop = ShouldEnterShopAfterCompletion();

            if (shouldEnterShop)
                EnterShopAfterCompletion(pendingNextLevelBranchIndex);
            else
                EnterNextLevelDirectly(pendingNextLevelBranchIndex);
        }

        // 处理奖励物品被获取事件
        private void OnRewardItemClaimed(RewardItemBase rewardItem)
        {
            if (!isShowingRewards) return;

            Debug.Log($"奖励物品已获取: {rewardItem?.ItemName}");

            // 不再自动隐藏奖励面板，等待玩家点击确认按钮
            // Debug.Log("已获取奖励，隐藏奖励面板");
            // HideRewardPanel();
        }

        // 检查是否应该显示奖励面板
        private bool ShouldShowRewardPanel()
        {
            // 统一从ConfigProvider获取游戏配置
            var provider = ConfigProvider.Instance;
            var gameConfig = provider ? provider.GetGameConfig() : null;
            if (gameConfig != null) return gameConfig.ShowRewardPanelAfterCompletion;

            // 如果无法获取配置，默认显示奖励面板
            Debug.LogWarning("LevelCompletionManager: 无法获取游戏配置，默认显示奖励面板");
            return true;
        }

        // 检查是否应该进入商店
        private bool ShouldEnterShopAfterCompletion()
        {
            if (!LevelStateManager.Instance)
            {
                Debug.LogWarning("LevelCompletionManager: LevelStateManager不存在，默认不进入商店");
                return false;
            }

            var currentLevelData = LevelStateManager.Instance.GetCurrentLevelData();
            if (currentLevelData != null) return currentLevelData.shouldEnterShop;

            Debug.LogWarning("LevelCompletionManager: 无法获取当前关卡数据，默认不进入商店");
            return false;
        }

        // 进入商店
        private void EnterShopAfterCompletion(int nextLevelBranchIndex)
        {
            var currentLevelName = GetCurrentLevelName();
            Debug.Log($"关卡完成后进入商店: {currentLevelName} -> Shop -> NextLevel(Branch: {nextLevelBranchIndex})");

            onEnteringShop?.Invoke(currentLevelName);

            // 准备进入商店场景，并保存下一关信息
            if (LevelStateManager.Instance)
                LevelStateManager.Instance.PrepareForShopWithNextLevel(nextLevelBranchIndex);

            // 进入商店场景
            if (SceneTransitionManager.Instance)
                SceneTransitionManager.Instance.EnterShop();
            else
                Debug.LogError("LevelCompletionManager: SceneTransitionManager不存在，无法进入商店");
        }

        // 直接进入下一关
        private void EnterNextLevelDirectly(int nextLevelBranchIndex)
        {
            var currentLevelName = GetCurrentLevelName();
            Debug.Log($"关卡完成后直接进入下一关: {currentLevelName} -> NextLevel(Branch: {nextLevelBranchIndex})");

            onEnteringNextLevel?.Invoke(currentLevelName, nextLevelBranchIndex);

            // 直接加载下一关
            if (LevelManager.Instance)
            {
                var loadSuccess = LevelManager.Instance.LoadNextLevel(nextLevelBranchIndex);
                if (loadSuccess)
                    Debug.Log($"成功直接加载下一关分支: {nextLevelBranchIndex}");
                else
                    Debug.LogWarning($"直接加载下一关分支失败: {nextLevelBranchIndex}");
            }
            else
            {
                Debug.LogError("LevelCompletionManager: LevelManager不存在，无法加载下一关");
            }
        }

        // 显示结算面板并结束游戏（胜利/失败）
        private void ShowGameResultAndEnd(bool isWin)
        {
            var resultUI = FindObjectOfType<GameResultUIController>(true);
            if (resultUI != null)
                resultUI.Show(isWin);
            else
                Debug.LogWarning("未找到GameResultUIController，无法显示结算面板");

            // 恢复游戏状态
            if (GameManager.Instance) GameManager.Instance.SetGameState(GameManager.GameState.Idle);
        }

        // 获取当前关卡名称
        private string GetCurrentLevelName()
        {
            if (LevelStateManager.Instance) return LevelStateManager.Instance.GetCurrentLevelName();

            if (LevelManager.Instance) return LevelManager.Instance.GetCurrentLevelName();

            return "";
        }

        // 获取当前关卡的下一关分支数量（用于验证分支索引）
        public int GetNextLevelBranchCount()
        {
            if (LevelStateManager.Instance) return LevelStateManager.Instance.GetNextLevelBranchCount();

            if (LevelManager.Instance) return LevelManager.Instance.GetNextLevelBranchCount();

            return 0;
        }

        // 验证分支索引是否有效
        public bool IsValidBranchIndex(int branchIndex)
        {
            var branchCount = GetNextLevelBranchCount();
            return branchIndex >= 0 && branchIndex < branchCount;
        }

        // 获取状态信息（用于调试）
        public string GetStatusInfo()
        {
            var currentLevel = GetCurrentLevelName();
            var shouldEnterShop = ShouldEnterShopAfterCompletion();
            var branchCount = GetNextLevelBranchCount();

            return
                $"当前关卡: {currentLevel}, 需要进入商店: {shouldEnterShop}, 下一关分支数: {branchCount}, 正在显示奖励: {isShowingRewards}";
        }

        // 手动隐藏奖励面板（供外部调用）
        public void ForceHideRewardPanel()
        {
            if (isShowingRewards) HideRewardPanel();
        }

        // 确认按钮点击处理（供奖励面板调用）
        public void OnRewardConfirmButtonClicked()
        {
            if (isShowingRewards)
            {
                Debug.Log("玩家点击确认按钮，隐藏奖励面板");
                HideRewardPanel();
            }
        }

        // 检查是否正在显示奖励
        public bool IsShowingRewards()
        {
            return isShowingRewards;
        }

        // 设置奖励UI控制器（供外部设置）
        public void SetRewardUIController(RewardUIController controller)
        {
            rewardUIController = controller;
        }

        // 处理金币奖励发放（已移除CoinRewardManager，现在通过奖励道具系统处理）
        private void HandleCoinReward()
        {
            // 金币奖励现在通过奖励道具系统处理，不再需要单独的管理器
            Debug.Log("金币奖励将通过奖励道具系统处理");
        }
    }
}