using System;
using System.Collections.Generic;
using System.IO;
using HappyHotel.Core.Singleton;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HappyHotel.GameManager
{
    // 轻量级关卡状态管理器，不依赖Grid，负责数据保存职责
    [ManagedSingleton(true)]
    public class LevelStateManager : SingletonBase<LevelStateManager>
    {
        // JSON配置文件路径
        private const string CONFIG_FILE_PATH = "Map Data/LevelConfig";

        // 关卡配置数据（从LevelManager移过来）
        [SerializeField] private List<LevelData> levelConfigs = new();
        [SerializeField] private LevelSettings levelSettings = new();
        private readonly Dictionary<string, LevelData> levelConfigDict = new();

        // 当前关卡信息（仅关卡名称和基本信息）
        private string currentLevelName = "";
        private int nextLevelBranchIndex;
        private string previousLevelName = "";
        private string sceneBeforeShop = "";

        // 从Shop返回后要进入的下一关信息
        private bool shouldLoadNextLevel;

        // 从Shop返回时需要的信息
        private bool shouldReturnToLevel;

        // 关卡状态改变事件
        public static event Action<string> onLevelStateChanged;

        protected override void OnSingletonAwake()
        {
            base.OnSingletonAwake();
            InitializeLevelConfigs();
            Debug.Log("LevelStateManager 初始化完成");
        }

        // 初始化关卡配置
        private void InitializeLevelConfigs()
        {
            // 清空字典
            levelConfigDict.Clear();

            // 尝试从JSON文件加载配置
            if (LoadLevelConfigFromJson())
            {
                Debug.Log("从JSON文件加载关卡配置成功");
            }
            else
            {
                Debug.LogError("从JSON文件加载关卡配置失败");
                return;
            }

            // 构建字典
            foreach (var levelData in levelConfigs)
                if (!levelConfigDict.ContainsKey(levelData.levelName))
                    levelConfigDict[levelData.levelName] = levelData;

            Debug.Log($"关卡状态管理器初始化完成，共加载 {levelConfigs.Count} 个关卡配置");
        }

        // 从JSON文件加载关卡配置
        private bool LoadLevelConfigFromJson()
        {
            try
            {
                // 从Resources加载JSON文件
                var configAsset = Resources.Load<TextAsset>(CONFIG_FILE_PATH);
                if (configAsset == null)
                {
                    Debug.LogWarning($"无法找到关卡配置文件: {CONFIG_FILE_PATH}");
                    return false;
                }

                // 解析JSON
                var configData = JsonUtility.FromJson<LevelConfigData>(configAsset.text);
                if (configData == null)
                {
                    Debug.LogError("解析关卡配置JSON失败");
                    return false;
                }

                // 应用配置
                levelConfigs.Clear();
                levelConfigs.AddRange(configData.levels);
                levelSettings = configData.settings ?? new LevelSettings();

                Debug.Log($"成功从JSON加载 {levelConfigs.Count} 个关卡配置");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"加载关卡配置JSON时发生错误: {e.Message}");
                return false;
            }
        }

        // 保存关卡配置到JSON文件（编辑器功能）
        public void SaveLevelConfigToJson()
        {
#if UNITY_EDITOR
            try
            {
                var configData = new LevelConfigData
                {
                    levels = new List<LevelData>(levelConfigs),
                    settings = levelSettings
                };

                var json = JsonUtility.ToJson(configData, true);
                var filePath = Path.Combine(Application.dataPath,
                    "Happy Hotel/Map/Resources/Map Data/LevelConfig.json");
                File.WriteAllText(filePath, json);

                Debug.Log($"关卡配置已保存到: {filePath}");
                AssetDatabase.Refresh();
            }
            catch (Exception e)
            {
                Debug.LogError($"保存关卡配置JSON时发生错误: {e.Message}");
            }
#else
            Debug.LogWarning("保存关卡配置功能仅在编辑器中可用");
#endif
        }

        // 设置当前关卡
        public void SetCurrentLevel(string levelName)
        {
            if (currentLevelName != levelName)
            {
                previousLevelName = currentLevelName;
                currentLevelName = levelName;
                onLevelStateChanged?.Invoke(currentLevelName);
                Debug.Log($"关卡状态更新: {levelName}");
            }
        }

        // 进入Shop场景前的准备
        public void PrepareForShop()
        {
            shouldReturnToLevel = !string.IsNullOrEmpty(currentLevelName);
            sceneBeforeShop = SceneManager.GetActiveScene().name;
            Debug.Log($"准备进入Shop，当前关卡: {currentLevelName}, 来源场景: {sceneBeforeShop}");
        }

        // 进入Shop场景前的准备（带下一关信息）
        public void PrepareForShopWithNextLevel(int branchIndex)
        {
            shouldReturnToLevel = !string.IsNullOrEmpty(currentLevelName);
            sceneBeforeShop = SceneManager.GetActiveScene().name;
            shouldLoadNextLevel = true;
            nextLevelBranchIndex = branchIndex;
            Debug.Log($"准备进入Shop，当前关卡: {currentLevelName}");
        }

        // 进入Shop场景
        public void EnterShop()
        {
            Debug.Log($"进入Shop场景，需要返回关卡: {shouldReturnToLevel}");
        }

        // 从Shop返回
        public void ExitShop()
        {
            if (shouldReturnToLevel && !string.IsNullOrEmpty(currentLevelName))
                Debug.Log($"从Shop返回，目标关卡: {currentLevelName}, 目标场景: {sceneBeforeShop}");
            // 这里不直接加载，而是提供信息给其他管理器使用
        }

        // 完成返回后清理状态
        public void CompleteReturn()
        {
            shouldReturnToLevel = false;
            sceneBeforeShop = "";
            shouldLoadNextLevel = false;
            nextLevelBranchIndex = 0;
            Debug.Log("完成从Shop返回的状态清理");
        }

        // 获取当前关卡名称
        public string GetCurrentLevelName()
        {
            return currentLevelName;
        }

        // 获取上一个关卡名称
        public string GetPreviousLevelName()
        {
            return previousLevelName;
        }

        // 是否需要返回关卡
        public bool ShouldReturnToLevel()
        {
            return shouldReturnToLevel;
        }

        // 获取Shop前的场景名称
        public string GetSceneBeforeShop()
        {
            return sceneBeforeShop;
        }

        // 是否需要加载下一关
        public bool ShouldLoadNextLevel()
        {
            return shouldLoadNextLevel;
        }

        // 获取下一关分支索引
        public int GetNextLevelBranchIndex()
        {
            return nextLevelBranchIndex;
        }

        // 清除关卡状态（用于重置游戏等情况）
        public void ClearLevelState()
        {
            currentLevelName = "";
            previousLevelName = "";
            shouldReturnToLevel = false;
            sceneBeforeShop = "";
            shouldLoadNextLevel = false;
            nextLevelBranchIndex = 0;
            Debug.Log("关卡状态已清除");
        }

        // 检查是否在Shop场景
        public bool IsInShopScene()
        {
            return SceneManager.GetActiveScene().name == "ShopScene";
        }

        // 获取关卡状态信息（用于调试）
        public string GetStateInfo()
        {
            return $"当前关卡: {currentLevelName}, 上一关卡: {previousLevelName}, " +
                   $"需要返回: {shouldReturnToLevel}, Shop前场景: {sceneBeforeShop}";
        }

        // ===== 关卡配置数据访问方法 =====

        // 获取关卡数据
        public LevelData GetLevelData(string levelName)
        {
            if (levelConfigDict.ContainsKey(levelName)) return levelConfigDict[levelName];
            return null;
        }

        // 获取当前关卡数据
        public LevelData GetCurrentLevelData()
        {
            if (string.IsNullOrEmpty(currentLevelName) || !levelConfigDict.ContainsKey(currentLevelName)) return null;

            return levelConfigDict[currentLevelName];
        }

        // 获取当前关卡的下一关分支数量
        public int GetNextLevelBranchCount()
        {
            if (string.IsNullOrEmpty(currentLevelName) || !levelConfigDict.ContainsKey(currentLevelName))
            {
                Debug.LogWarning("[LevelStateManager] GetNextLevelBranchCount 当前关卡无效");
                return 0;
            }

            return levelConfigDict[currentLevelName].nextLevels.Count;
        }

        // 获取当前关卡的下一关分支列表
        public List<string> GetNextLevelBranches()
        {
            if (string.IsNullOrEmpty(currentLevelName) || !levelConfigDict.ContainsKey(currentLevelName))
                return new List<string>();

            return new List<string>(levelConfigDict[currentLevelName].nextLevels);
        }

        // 获取所有起始关卡
        public List<LevelData> GetStartLevels()
        {
            var startLevels = new List<LevelData>();
            foreach (var level in levelConfigs)
                if (level.isStartLevel)
                    startLevels.Add(level);

            return startLevels;
        }

        // 获取关卡设置
        public LevelSettings GetLevelSettings()
        {
            return levelSettings;
        }

        // 获取所有关卡配置
        public List<LevelData> GetAllLevelConfigs()
        {
            return new List<LevelData>(levelConfigs);
        }

        // 添加关卡配置
        public void AddLevelConfig(string levelName, List<string> nextLevels = null)
        {
            var newLevel = new LevelData(levelName);
            if (nextLevels != null) newLevel.nextLevels.AddRange(nextLevels);

            levelConfigs.Add(newLevel);
            levelConfigDict[levelName] = newLevel;

            Debug.Log($"添加关卡配置: {levelName}");
        }

        // 设置关卡的下一关分支
        public void SetLevelNextBranches(string levelName, List<string> nextLevels)
        {
            if (levelConfigDict.ContainsKey(levelName))
            {
                levelConfigDict[levelName].nextLevels.Clear();
                levelConfigDict[levelName].nextLevels.AddRange(nextLevels);

                // 同步到配置列表
                for (var i = 0; i < levelConfigs.Count; i++)
                    if (levelConfigs[i].levelName == levelName)
                    {
                        levelConfigs[i].nextLevels.Clear();
                        levelConfigs[i].nextLevels.AddRange(nextLevels);
                        break;
                    }

                Debug.Log($"设置关卡 {levelName} 的下一关分支: {string.Join(", ", nextLevels)}");
            }
            else
            {
                Debug.LogWarning($"关卡配置不存在: {levelName}");
            }
        }

        // 重新加载关卡配置
        public void ReloadLevelConfigs()
        {
            levelConfigs.Clear();
            InitializeLevelConfigs();
        }

        // 获取下一关名称（通过分支索引）
        public string GetNextLevelName(int branchIndex = 0)
        {
            if (string.IsNullOrEmpty(currentLevelName))
            {
                Debug.LogWarning("[LevelStateManager] GetNextLevelName 当前关卡为空");
                return null;
            }

            // 查找当前关卡配置
            if (!levelConfigDict.ContainsKey(currentLevelName))
            {
                Debug.LogWarning("[LevelStateManager] GetNextLevelName 未找到关卡配置");
                return null;
            }

            var currentLevel = levelConfigDict[currentLevelName];

            // 检查分支索引是否有效
            if (branchIndex < 0 || branchIndex >= currentLevel.nextLevels.Count)
            {
                Debug.LogWarning("[LevelStateManager] GetNextLevelName 分支索引无效");
                return null;
            }

            return currentLevel.nextLevels[branchIndex];
        }

        // 获取默认起始关卡
        public string GetDefaultStartLevel()
        {
            if (!string.IsNullOrEmpty(levelSettings.defaultStartLevel)) return levelSettings.defaultStartLevel;

            if (levelConfigs.Count > 0)
            {
                // 查找第一个起始关卡
                var startLevels = GetStartLevels();
                if (startLevels.Count > 0) return startLevels[0].levelName;

                return levelConfigs[0].levelName;
            }

            return null;
        }

        // 关卡数据结构（从LevelManager移过来）
        [Serializable]
        public class LevelData
        {
            public string levelName;
            public string displayName;
            public string description;
            public List<string> nextLevels = new(); // 下一关分支列表
            public bool isStartLevel;
            public bool shouldEnterShop; // 通关后是否进入商店
            public bool isFinalLevel; // 是否为最终关卡（通关后直接结算）

            public LevelData(string name)
            {
                levelName = name;
                displayName = name;
                description = "";
                isStartLevel = false;
                shouldEnterShop = false; // 默认不进入商店
                isFinalLevel = false; // 默认不是最终关卡
            }
        }

        [Serializable]
        public class LevelConfigData
        {
            public List<LevelData> levels = new();
            public LevelSettings settings = new();
        }

        [Serializable]
        public class LevelSettings
        {
            public string defaultStartLevel;
        }
    }
}