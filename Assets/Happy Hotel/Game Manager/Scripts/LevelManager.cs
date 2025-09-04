using System;
using System.Collections;
using System.Collections.Generic;
using HappyHotel.Core.Grid;
using HappyHotel.Core.Singleton;
using HappyHotel.Inventory;
using HappyHotel.Map;
using HappyHotel.Reward;
using UnityEngine;

namespace HappyHotel.GameManager
{
    [ManagedSingleton(SceneLoadMode.Exclude, "MapEditScene", "ShopScene", "MainMenu")]
    public class LevelManager : SingletonBase<LevelManager>
    {
        // 当前关卡名称（LevelManager只管理当前加载的关卡）
        private string currentLevelName = "";

        public bool IsInitialized { get; private set; }

        // 关卡进度改变事件
        public static event Action<string> onLevelChanged;

        protected override void OnSingletonAwake()
        {
            base.OnSingletonAwake();
            Debug.Log("LevelManager 初始化完成");

            IsInitialized = true;
        }

        // 检查关卡初始出口装置状态的协程
        private IEnumerator CheckInitialExitDeviceState()
        {
            // 等待一帧，确保所有对象都已初始化完成
            yield return null;

            // 调用GameManager的敌人检查方法来设置出口装置状态
            if (GameManager.Instance)
            {
                GameManager.Instance.CheckAllEnemiesDead();
                Debug.Log("关卡加载完成后检查出口装置初始状态");
            }
        }

        // 加载指定关卡
        public bool LoadLevel(string levelName)
        {
            if (string.IsNullOrEmpty(levelName))
            {
                Debug.LogError("关卡名称不能为空");
                return false;
            }

            // 检查关卡文件是否存在
            if (!IsLevelExists(levelName))
            {
                Debug.LogError($"无法找到关卡文件: {levelName}");
                return false;
            }

            // 清空所有 GridObject
            if (GridObjectManager.Instance) GridObjectManager.Instance.ClearAllObjects();

            // 清空动态奖励（进入新关卡时）
            if (RewardClaimController.Instance) RewardClaimController.Instance.ClearDynamicRewards();

            // 重置回合系统
            if (TurnManager.Instance) TurnManager.Instance.ResetTurn();

            // 使用MapStorageManager加载地图
            if (MapStorageManager.Instance)
            {
                var loadSuccess = MapStorageManager.Instance.LoadMap(levelName);
                if (loadSuccess)
                {
                    currentLevelName = levelName;
                    onLevelChanged?.Invoke(currentLevelName);

                    // 通知LevelStateManager更新状态
                    if (LevelStateManager.Instance) LevelStateManager.Instance.SetCurrentLevel(levelName);

                    // 重置装备状态（包括消耗型装备和刷新状态）
                    if (EquipmentSpawnManager.Instance) EquipmentSpawnManager.Instance.ResetAllStates();

                    // 重置卡牌系统状态
                    if (CardDrawManager.Instance) CardDrawManager.Instance.ResetForNewLevel();

                    // 设置游戏状态为待机
                    if (GameManager.Instance) GameManager.Instance.SetGameState(GameManager.GameState.Idle);

                    // 关卡加载完成后，检查出口装置的初始状态
                    // 如果关卡没有敌人，出口装置应该立即显示为可通过
                    if (GameManager.Instance)
                        // 使用协程延迟一帧执行，确保所有对象都已正确初始化
                        StartCoroutine(CheckInitialExitDeviceState());

                    // 初始化波次刷新管理器
                    if (WaveSpawnManager.Instance && MapStorageManager.Instance)
                    {
                        WaveSpawnManager.Instance.InitializeFromMap(MapStorageManager.Instance.GetCurrentMapData());
                        Debug.Log("波次刷新管理器已初始化");
                    }

                    // 金币奖励配置现在由RewardController在关卡完成时直接读取
                    Debug.Log("金币奖励配置将在关卡完成时读取");

                    Debug.Log("关卡初始化完成");

                    // 关卡加载完成后，开始第一回合
                    if (TurnManager.Instance) TurnManager.Instance.StartFirstTurn();

                    Debug.Log($"成功加载关卡: {levelName}");
                    return true;
                }

                Debug.LogError($"MapStorageManager加载关卡失败: {levelName}");
                return false;
            }

            Debug.LogError("MapStorageManager实例不存在");
            return false;
        }

        // 加载下一关（指定分支）
        public bool LoadNextLevel(int branchIndex = 0)
        {
            if (!LevelStateManager.Instance)
            {
                Debug.LogError("LevelStateManager不存在，无法加载下一关");
                return false;
            }

            // 从LevelStateManager获取下一关名称
            var nextLevelName = LevelStateManager.Instance.GetNextLevelName(branchIndex);

            if (string.IsNullOrEmpty(nextLevelName))
            {
                Debug.LogWarning($"无法获取下一关名称，分支索引: {branchIndex}");
                return false;
            }

            // 加载下一关
            return LoadLevel(nextLevelName);
        }

        // 获取当前关卡名称
        public string GetCurrentLevelName()
        {
            // 优先返回LevelStateManager中的状态，如果不存在则返回本地状态
            if (LevelStateManager.Instance)
            {
                var stateManagerLevel = LevelStateManager.Instance.GetCurrentLevelName();
                if (!string.IsNullOrEmpty(stateManagerLevel)) return stateManagerLevel;
            }

            return currentLevelName;
        }

        // 获取当前关卡数据
        public LevelStateManager.LevelData GetCurrentLevelData()
        {
            if (LevelStateManager.Instance) return LevelStateManager.Instance.GetCurrentLevelData();
            return null;
        }

        // 获取关卡数据
        public LevelStateManager.LevelData GetLevelData(string levelName)
        {
            if (LevelStateManager.Instance) return LevelStateManager.Instance.GetLevelData(levelName);
            return null;
        }

        // 获取当前关卡的下一关分支数量
        public int GetNextLevelBranchCount()
        {
            if (LevelStateManager.Instance) return LevelStateManager.Instance.GetNextLevelBranchCount();
            return 0;
        }

        // 获取当前关卡的下一关分支列表
        public List<string> GetNextLevelBranches()
        {
            if (LevelStateManager.Instance) return LevelStateManager.Instance.GetNextLevelBranches();
            return new List<string>();
        }

        // 获取所有起始关卡
        public List<LevelStateManager.LevelData> GetStartLevels()
        {
            if (LevelStateManager.Instance) return LevelStateManager.Instance.GetStartLevels();
            return new List<LevelStateManager.LevelData>();
        }

        // 添加关卡配置（委托给LevelStateManager）
        public void AddLevelConfig(string levelName, List<string> nextLevels = null)
        {
            if (LevelStateManager.Instance)
                LevelStateManager.Instance.AddLevelConfig(levelName, nextLevels);
            else
                Debug.LogError("LevelStateManager不存在，无法添加关卡配置");
        }

        // 设置关卡的下一关分支（委托给LevelStateManager）
        public void SetLevelNextBranches(string levelName, List<string> nextLevels)
        {
            if (LevelStateManager.Instance)
                LevelStateManager.Instance.SetLevelNextBranches(levelName, nextLevels);
            else
                Debug.LogError("LevelStateManager不存在，无法设置关卡分支");
        }

        // 重新加载关卡配置（委托给LevelStateManager）
        public void ReloadLevelConfigs()
        {
            if (LevelStateManager.Instance)
                LevelStateManager.Instance.ReloadLevelConfigs();
            else
                Debug.LogError("LevelStateManager不存在，无法重新加载关卡配置");
        }

        // 保存关卡配置到JSON文件（委托给LevelStateManager）
        public void SaveLevelConfigToJson()
        {
            if (LevelStateManager.Instance)
                LevelStateManager.Instance.SaveLevelConfigToJson();
            else
                Debug.LogError("LevelStateManager不存在，无法保存关卡配置");
        }

        // 检查关卡文件是否存在
        public bool IsLevelExists(string levelName)
        {
            if (MapStorageManager.Instance)
            {
                var availableMaps = MapStorageManager.Instance.GetAvailableMaps();
                foreach (var mapName in availableMaps)
                    if (mapName == levelName)
                        return true;
            }

            return false;
        }

        // 开始游戏（加载第一关）
        public bool StartGame(string firstLevelName = null)
        {
            // 如果没有指定第一关，从LevelStateManager获取默认起始关卡
            if (string.IsNullOrEmpty(firstLevelName))
            {
                if (LevelStateManager.Instance) firstLevelName = LevelStateManager.Instance.GetDefaultStartLevel();

                if (string.IsNullOrEmpty(firstLevelName))
                {
                    Debug.LogError("没有可用的关卡配置");
                    return false;
                }
            }

            return LoadLevel(firstLevelName);
        }

        // 重新开始当前关卡
        public bool RestartCurrentLevel()
        {
            var currentLevel = GetCurrentLevelName();
            if (string.IsNullOrEmpty(currentLevel))
            {
                Debug.LogWarning("当前没有加载任何关卡，无法重新开始");
                return false;
            }

            return LoadLevel(currentLevel);
        }

        // 获取关卡设置
        public LevelStateManager.LevelSettings GetLevelSettings()
        {
            if (LevelStateManager.Instance) return LevelStateManager.Instance.GetLevelSettings();
            return null;
        }

        // 获取所有关卡配置
        public List<LevelStateManager.LevelData> GetAllLevelConfigs()
        {
            if (LevelStateManager.Instance) return LevelStateManager.Instance.GetAllLevelConfigs();
            return new List<LevelStateManager.LevelData>();
        }
    }
}