using System;
using System.Collections;
using HappyHotel.Core.Singleton;
using UnityEngine;

namespace HappyHotel.GameManager
{
    public class GameSceneInitializer : SingletonBase<GameSceneInitializer>
    {
        [Header("游戏配置")] [SerializeField] [Tooltip("使用的默认角色配置（Resources路径）")]
        private string defaultCharacterConfigPath = "CharacterSelectionConfigs/DefaultCharacter";

        [SerializeField] [Tooltip("使用的默认关卡名称")]
        private string defaultLevelName = "Tutorial";

        [Header("自动开始游戏")] [SerializeField] [Tooltip("是否在编辑器中自动开始游戏")]
        private bool autoStartGameInEditor = true;

        [Header("调试信息")] [SerializeField] [Tooltip("是否显示详细的初始化日志")]
        private bool showDetailedLogs = true;

        // 初始化状态

        /// <summary>
        ///     获取初始化状态
        /// </summary>
        public bool IsInitialized { get; private set; }

        protected override void OnSingletonAwake()
        {
            base.OnSingletonAwake();

            // 延迟初始化，确保所有单例都已加载
            StartCoroutine(DelayedInitialization());
        }

        /// <summary>
        ///     延迟初始化协程
        /// </summary>
        private IEnumerator DelayedInitialization()
        {
            // 等待一帧，确保所有单例都已初始化
            yield return null;

            // 执行初始化
            InitializeGameScene();
        }

        /// <summary>
        ///     初始化游戏场景
        /// </summary>
        private void InitializeGameScene()
        {
            if (IsInitialized)
            {
                LogMessage("GameScene已经初始化过，跳过重复初始化");
                return;
            }

            LogMessage("开始初始化GameScene...");

            try
            {
                // 执行独立播放模式初始化
                InitializeStandalone();

                // 加载关卡
                LoadInitialLevel();

                // 标记初始化完成
                IsInitialized = true;

                LogMessage("GameScene初始化完成");

                // 在编辑器中自动开始游戏
                if (autoStartGameInEditor && Application.isEditor) StartCoroutine(AutoStartGame());
            }
            catch (Exception e)
            {
                Debug.LogError($"GameScene初始化失败: {e.Message}");
            }
        }

        /// <summary>
        ///     独立播放模式的初始化
        /// </summary>
        private void InitializeStandalone()
        {
            LogMessage("执行游戏初始化流程");

            // 加载默认角色配置
            var defaultConfig = LoadDefaultCharacterConfig();

            if (defaultConfig != null)
            {
                // 使用默认配置初始化新游戏
                if (NewGameInitializer.Instance != null)
                {
                    NewGameInitializer.Instance.InitializeNewGame(defaultConfig);
                    LogMessage($"使用默认角色配置初始化新游戏: {defaultConfig.CharacterName}");
                }
                else
                {
                    Debug.LogError("NewGameInitializer实例不存在，无法初始化新游戏");
                }
            }
            else
            {
                Debug.LogError("无法加载默认角色配置，游戏可能无法正常运行");
            }
        }

        /// <summary>
        ///     加载默认角色配置
        /// </summary>
        private CharacterSelectionConfig LoadDefaultCharacterConfig()
        {
            if (string.IsNullOrEmpty(defaultCharacterConfigPath))
            {
                Debug.LogError("默认角色配置路径为空");
                return null;
            }

            try
            {
                var config = Resources.Load<CharacterSelectionConfig>(defaultCharacterConfigPath);

                if (config != null)
                {
                    LogMessage($"成功加载默认角色配置: {config.CharacterName}");
                    return config;
                }

                Debug.LogError($"无法在Resources中找到默认角色配置: {defaultCharacterConfigPath}");
                return null;
            }
            catch (Exception e)
            {
                Debug.LogError($"加载默认角色配置时发生异常: {e.Message}");
                return null;
            }
        }

        /// <summary>
        ///     加载初始关卡
        /// </summary>
        private void LoadInitialLevel()
        {
            if (LevelManager.Instance != null)
            {
                var success = LevelManager.Instance.LoadLevel(defaultLevelName);
                if (success)
                    LogMessage($"成功加载初始关卡: {defaultLevelName}");
                else
                    Debug.LogError($"加载初始关卡失败: {defaultLevelName}");
            }
            else
            {
                Debug.LogError("LevelManager实例不存在，无法加载关卡");
            }
        }

        /// <summary>
        ///     自动开始游戏协程
        /// </summary>
        private IEnumerator AutoStartGame()
        {
            // 等待一帧，确保所有初始化完成
            yield return null;

            // 等待关卡加载完成
            yield return new WaitUntil(() => LevelManager.Instance != null && LevelManager.Instance.IsInitialized);

            // 开始游戏
            if (TurnManager.Instance != null)
            {
                TurnManager.Instance.StartFirstTurn();
                LogMessage("自动开始游戏（第一回合）");
            }
        }

        /// <summary>
        ///     输出日志信息
        /// </summary>
        private void LogMessage(string message)
        {
            if (showDetailedLogs) Debug.Log($"[GameSceneInitializer] {message}");
        }

        /// <summary>
        ///     手动重新初始化（供调试使用）
        /// </summary>
        [ContextMenu("重新初始化")]
        public void Reinitialize()
        {
            IsInitialized = false;
            StartCoroutine(DelayedInitialization());
        }

        /// <summary>
        ///     设置默认角色配置路径
        /// </summary>
        public void SetDefaultCharacterConfigPath(string path)
        {
            defaultCharacterConfigPath = path;
            LogMessage($"设置默认角色配置路径: {path}");
        }

        /// <summary>
        ///     设置默认关卡名称
        /// </summary>
        public void SetDefaultLevelName(string levelName)
        {
            defaultLevelName = levelName;
            LogMessage($"设置默认关卡名称: {levelName}");
        }
    }
}