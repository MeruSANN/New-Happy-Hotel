using System;
using System.Collections;
using HappyHotel.Core.Singleton;
using HappyHotel.GameManager;
using HappyHotel.Shop;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HappyHotel.Core.Scene
{
    [ManagedSingleton(true)]
    public class SceneTransitionManager : SingletonBase<SceneTransitionManager>
    {
        private bool isTransitioning;

        // 场景转换事件
        public static event Action<string> onSceneTransitionStart;
        public static event Action<string> onSceneTransitionComplete;

        protected override void OnSingletonAwake()
        {
            base.OnSingletonAwake();
            Debug.Log("SceneTransitionManager 初始化完成");
        }

        // 进入Shop场景
        public void EnterShop()
        {
            if (isTransitioning)
            {
                Debug.LogWarning("场景转换进行中，无法进入Shop");
                return;
            }

            Debug.Log("准备进入Shop场景");

            // 保存MainCharacter血量
            if (CharacterHealthManager.Instance) CharacterHealthManager.Instance.SaveCurrentHealth();

            // 通知LevelStateManager准备进入Shop
            if (LevelStateManager.Instance) LevelStateManager.Instance.PrepareForShop();

            StartCoroutine(TransitionToShop());
        }

        // 从Shop返回游戏场景
        public void ExitShop()
        {
            if (isTransitioning)
            {
                Debug.LogWarning("场景转换进行中，无法退出Shop");
                return;
            }

            if (!LevelStateManager.Instance)
            {
                Debug.LogError("LevelStateManager不存在，无法退出Shop");
                return;
            }

            var targetScene = LevelStateManager.Instance.GetSceneBeforeShop();
            var targetLevel = LevelStateManager.Instance.GetCurrentLevelName();

            Debug.Log($"准备从Shop返回，目标场景: {targetScene}, 目标关卡: {targetLevel}");

            // 通知LevelStateManager开始退出Shop
            LevelStateManager.Instance.ExitShop();

            StartCoroutine(TransitionFromShop(targetScene, targetLevel));
        }

        // 转换到Shop场景的协程
        private IEnumerator TransitionToShop()
        {
            isTransitioning = true;
            onSceneTransitionStart?.Invoke("ShopScene");

            // 加载Shop场景
            var asyncLoad = SceneManager.LoadSceneAsync("ShopScene");

            while (!asyncLoad.isDone) yield return null;

            // 等待一帧确保场景完全加载
            yield return null;

            // 强制刷新单例状态
            if (SingletonManager.Instance) SingletonManager.Instance.RefreshCurrentSceneSingletons();

            // 重新加载配置（确保场景切换后配置正确）
            ReloadConfigurations();

            // 通知LevelStateManager已进入Shop
            if (LevelStateManager.Instance) LevelStateManager.Instance.EnterShop();

            // 刷新商店物品
            RefreshShopOnSceneLoad();

            isTransitioning = false;
            onSceneTransitionComplete?.Invoke("ShopScene");
            Debug.Log("成功进入Shop场景");
        }

        // 从Shop返回的协程
        private IEnumerator TransitionFromShop(string targetScene, string targetLevel)
        {
            isTransitioning = true;
            onSceneTransitionStart?.Invoke(targetScene);

            // 如果目标场景为空，默认返回SampleScene
            if (string.IsNullOrEmpty(targetScene))
            {
                targetScene = "SampleScene";
                Debug.LogWarning("目标场景为空，默认返回SampleScene");
            }

            // 加载目标场景
            var asyncLoad = SceneManager.LoadSceneAsync(targetScene);

            while (!asyncLoad.isDone) yield return null;

            // 等待一帧确保场景完全加载
            yield return null;

            // 强制刷新单例状态
            if (SingletonManager.Instance) SingletonManager.Instance.RefreshCurrentSceneSingletons();

            // 重新加载配置（确保场景切换后配置正确）
            ReloadConfigurations();

            // 检查是否需要加载下一关
            if (LevelStateManager.Instance && LevelStateManager.Instance.ShouldLoadNextLevel())
            {
                // 加载下一关
                var branchIndex = LevelStateManager.Instance.GetNextLevelBranchIndex();
                yield return StartCoroutine(WaitForManagersAndLoadNextLevel(branchIndex));
            }
            else if (!string.IsNullOrEmpty(targetLevel))
            {
                // 加载原关卡
                yield return StartCoroutine(WaitForManagersAndLoadLevel(targetLevel));
            }

            // 通知LevelStateManager完成返回
            if (LevelStateManager.Instance) LevelStateManager.Instance.CompleteReturn();

            isTransitioning = false;
            onSceneTransitionComplete?.Invoke(targetScene);
            Debug.Log($"成功返回场景: {targetScene}");
        }

        // 等待管理器初始化并加载关卡
        private IEnumerator WaitForManagersAndLoadLevel(string levelName, bool isNewGame = false)
        {
            // 等待关键管理器初始化
            var timeout = 5f; // 5秒超时
            var elapsed = 0f;

            while (elapsed < timeout)
            {
                if (LevelManager.Instance && LevelManager.Instance.IsInitialized) break;

                elapsed += Time.deltaTime;
                yield return null;
            }

            if (elapsed >= timeout)
            {
                Debug.LogError("等待LevelManager初始化超时");
                yield break;
            }

            // 加载关卡
            if (LevelManager.Instance)
            {
                var loadSuccess = LevelManager.Instance.LoadLevel(levelName);
                if (loadSuccess)
                    Debug.Log($"成功从Shop返回并加载关卡: {levelName}");
                else
                    Debug.LogError($"从Shop返回后加载关卡失败: {levelName}");
            }
        }

        // 等待管理器初始化并加载下一关
        private IEnumerator WaitForManagersAndLoadNextLevel(int branchIndex)
        {
            // 等待关键管理器初始化
            var timeout = 5f; // 5秒超时
            var elapsed = 0f;

            while (elapsed < timeout)
            {
                if (LevelManager.Instance && LevelManager.Instance.IsInitialized) break;

                elapsed += Time.deltaTime;
                yield return null;
            }

            if (elapsed >= timeout)
            {
                Debug.LogError("等待LevelManager初始化超时");
                yield break;
            }

            // 加载下一关
            if (LevelManager.Instance)
            {
                var loadSuccess = LevelManager.Instance.LoadNextLevel(branchIndex);
                if (loadSuccess)
                    Debug.Log($"成功从Shop返回并加载下一关分支: {branchIndex}");
                else
                    Debug.LogError($"从Shop返回后加载下一关分支失败: {branchIndex}");
            }
        }

        // 开始新游戏（从主菜单跳转到GameScene并加载第一关）
        public void StartNewGame(string targetScene = "SampleScene", CharacterSelectionConfig characterConfig = null)
        {
            if (isTransitioning)
            {
                Debug.LogWarning("场景转换进行中，无法开始新游戏");
                return;
            }

            if (!LevelStateManager.Instance)
            {
                Debug.LogError("LevelStateManager不存在，无法开始新游戏");
                return;
            }

            // 获取第一关名称
            var firstLevelName = LevelStateManager.Instance.GetDefaultStartLevel();
            if (string.IsNullOrEmpty(firstLevelName))
            {
                Debug.LogError("无法获取默认起始关卡");
                return;
            }

            Debug.Log($"开始新游戏，目标场景: {targetScene}, 第一关: {firstLevelName}");

            // 清除LevelStateManager中的旧状态
            LevelStateManager.Instance.ClearLevelState();

            StartCoroutine(StartNewGameTransition(targetScene, firstLevelName, characterConfig));
        }

        // 开始新游戏的协程
        private IEnumerator StartNewGameTransition(string targetScene, string firstLevelName,
            CharacterSelectionConfig characterConfig)
        {
            isTransitioning = true;
            onSceneTransitionStart?.Invoke(targetScene);

            // 1. 加载目标场景
            var asyncLoad = SceneManager.LoadSceneAsync(targetScene);
            while (!asyncLoad.isDone) yield return null;
            yield return null; // 等待一帧确保场景完全加载

            // 2. 强制刷新单例状态
            if (SingletonManager.Instance) SingletonManager.Instance.RefreshCurrentSceneSingletons();

            // 3. 重新加载配置
            ReloadConfigurations();

            // 4. 初始化新游戏内容（添加初始卡牌、装备等）
            // 必须在加载关卡之前执行，以确保背包里有东西
            if (NewGameInitializer.Instance != null)
                NewGameInitializer.Instance.InitializeNewGame(characterConfig);
            else
                Debug.LogWarning("NewGameInitializer实例不存在，跳过新游戏初始化");

            // 5. 加载关卡并重置所有系统
            yield return StartCoroutine(WaitForManagersAndLoadLevel(firstLevelName));

            isTransitioning = false;
            onSceneTransitionComplete?.Invoke(targetScene);
            Debug.Log($"成功开始新游戏，场景: {targetScene}, 关卡: {firstLevelName}");
        }

        // 直接切换场景（不涉及Shop逻辑）
        public void SwitchScene(string sceneName)
        {
            if (isTransitioning)
            {
                Debug.LogWarning("场景转换进行中，无法切换场景");
                return;
            }

            StartCoroutine(DirectSceneTransition(sceneName));
        }

        // 直接场景转换的协程
        private IEnumerator DirectSceneTransition(string sceneName)
        {
            isTransitioning = true;
            onSceneTransitionStart?.Invoke(sceneName);

            var asyncLoad = SceneManager.LoadSceneAsync(sceneName);

            while (!asyncLoad.isDone) yield return null;

            yield return null;

            // 强制刷新单例状态
            if (SingletonManager.Instance) SingletonManager.Instance.RefreshCurrentSceneSingletons();

            isTransitioning = false;
            onSceneTransitionComplete?.Invoke(sceneName);
            Debug.Log($"成功切换到场景: {sceneName}");
        }

        // 获取当前是否在转换中
        public bool IsTransitioning()
        {
            return isTransitioning;
        }

        // 获取当前场景名称
        public string GetCurrentSceneName()
        {
            return SceneManager.GetActiveScene().name;
        }

        // 重新加载所有配置
        private void ReloadConfigurations()
        {
            // 仅重载统一的配置提供者，其余模块在各自OnEnable或订阅事件中自行应用
            if (ConfigProvider.Instance) ConfigProvider.Instance.ReloadConfig();

            Debug.Log("场景切换后配置已重新加载（ConfigProvider）");
        }

        // 手动刷新商店（公共方法，用于测试或外部调用）
        public void ManualRefreshShop()
        {
            RefreshShopOnSceneLoad();
        }

        // 在商店场景加载后刷新商店物品
        private void RefreshShopOnSceneLoad()
        {
            Debug.Log("开始刷新商店物品...");

            // 等待ShopController初始化
            if (ShopController.Instance)
            {
                // 立即刷新商店
                ShopController.Instance.RefreshShop();
                Debug.Log("商店物品已刷新");
            }
            else
            {
                // 如果ShopController还没有初始化，启动协程等待
                StartCoroutine(WaitForShopControllerAndRefresh());
            }
        }

        // 等待ShopController初始化并刷新商店
        private IEnumerator WaitForShopControllerAndRefresh()
        {
            Debug.Log("等待ShopController初始化...");

            var timeout = 5f; // 5秒超时
            var elapsed = 0f;

            while (elapsed < timeout)
            {
                if (ShopController.Instance)
                {
                    // ShopController已初始化，刷新商店
                    ShopController.Instance.RefreshShop();
                    Debug.Log("ShopController初始化完成，商店物品已刷新");
                    yield break;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            Debug.LogError("等待ShopController初始化超时，无法刷新商店物品");
        }
    }
}