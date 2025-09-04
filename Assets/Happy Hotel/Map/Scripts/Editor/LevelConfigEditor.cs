#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HappyHotel.GameManager;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace HappyHotel.Map.Editor
{
    public class LevelConfigEditor : OdinEditorWindow
    {
        // 配置文件路径
        private const string CONFIG_FILE_PATH = "Assets/Happy Hotel/Map/Resources/Map Data/LevelConfig.json";
        private const string MAP_DATA_FOLDER = "Assets/StreamingAssets/Maps";

        // 当前配置数据
        [TitleGroup("关卡配置")] [InlineProperty] [HideLabel]
        public LevelConfigData configData = new();

        // 可用地图列表
        [TitleGroup("可用地图")] [ReadOnly] [ListDrawerSettings(ShowIndexLabels = true)]
        public List<string> availableMaps = new();

        protected override void OnEnable()
        {
            base.OnEnable();
            RefreshAvailableMaps();
            LoadConfiguration();
        }

        [MenuItem("Happy Hotel/关卡配置编辑器")]
        public static void ShowWindow()
        {
            var window = GetWindow<LevelConfigEditor>();
            window.titleContent = new GUIContent("关卡配置编辑器");
            window.Show();
        }

        [TitleGroup("操作")]
        [HorizontalGroup("操作/Buttons")]
        [Button("刷新地图列表", ButtonSizes.Medium)]
        private void RefreshAvailableMaps()
        {
            availableMaps.Clear();

            if (Directory.Exists(MAP_DATA_FOLDER))
            {
                var mapFiles = Directory.GetFiles(MAP_DATA_FOLDER, "*.happymap");
                foreach (var filePath in mapFiles)
                {
                    var fileName = Path.GetFileNameWithoutExtension(filePath);
                    availableMaps.Add(fileName);
                }
            }

            availableMaps.Sort();
            Debug.Log($"发现 {availableMaps.Count} 个地图文件: {string.Join(", ", availableMaps)}");
        }

        [HorizontalGroup("操作/Buttons")]
        [Button("加载配置", ButtonSizes.Medium)]
        private void LoadConfiguration()
        {
            try
            {
                if (File.Exists(CONFIG_FILE_PATH))
                {
                    var jsonContent = File.ReadAllText(CONFIG_FILE_PATH);
                    var loadedData = JsonUtility.FromJson<LevelStateManager.LevelConfigData>(jsonContent);

                    // 转换数据格式
                    configData = new LevelConfigData();
                    configData.levels.Clear();

                    foreach (var level in loadedData.levels)
                    {
                        var newLevel = new LevelData
                        {
                            levelName = level.levelName,
                            displayName = level.displayName,
                            description = level.description,
                            nextLevels = new List<string>(level.nextLevels),
                            isStartLevel = level.isStartLevel,
                            shouldEnterShop = level.shouldEnterShop,
                            isFinalLevel = level.isFinalLevel
                        };
                        configData.levels.Add(newLevel);
                    }

                    configData.settings = new LevelSettings
                    {
                        defaultStartLevel = loadedData.settings.defaultStartLevel
                    };

                    Debug.Log($"成功加载配置，包含 {configData.levels.Count} 个关卡");
                }
                else
                {
                    Debug.LogWarning($"配置文件不存在: {CONFIG_FILE_PATH}");
                    configData = new LevelConfigData();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"加载配置失败: {e.Message}");
                EditorUtility.DisplayDialog("加载失败", $"加载配置文件时发生错误：{e.Message}", "确定");
            }
        }

        [HorizontalGroup("操作/Buttons")]
        [Button("保存配置", ButtonSizes.Medium)]
        private void SaveConfiguration()
        {
            try
            {
                // 转换数据格式
                var saveData = new LevelStateManager.LevelConfigData();
                saveData.levels.Clear();

                foreach (var level in configData.levels)
                {
                    var newLevel = new LevelStateManager.LevelData(level.levelName)
                    {
                        displayName = level.displayName,
                        description = level.description,
                        nextLevels = new List<string>(level.nextLevels),
                        isStartLevel = level.isStartLevel,
                        shouldEnterShop = level.shouldEnterShop,
                        isFinalLevel = level.isFinalLevel
                    };
                    saveData.levels.Add(newLevel);
                }

                saveData.settings = new LevelStateManager.LevelSettings
                {
                    defaultStartLevel = configData.settings.defaultStartLevel
                };

                // 序列化为JSON并保存
                var jsonContent = JsonUtility.ToJson(saveData, true);
                File.WriteAllText(CONFIG_FILE_PATH, jsonContent);
                AssetDatabase.Refresh();

                Debug.Log($"配置已保存到: {CONFIG_FILE_PATH}");
                EditorUtility.DisplayDialog("保存成功", "关卡配置已成功保存！", "确定");
            }
            catch (Exception e)
            {
                Debug.LogError($"保存配置失败: {e.Message}");
                EditorUtility.DisplayDialog("保存失败", $"保存配置文件时发生错误：{e.Message}", "确定");
            }
        }

        [TitleGroup("快速操作")]
        [HorizontalGroup("快速操作/Row1")]
        [Button("添加所有地图为关卡")]
        private void AddAllMapsAsLevels()
        {
            var addedCount = 0;

            foreach (var mapName in availableMaps)
                // 检查是否已存在
                if (!configData.levels.Any(l => l.levelName == mapName))
                {
                    var newLevel = new LevelData
                    {
                        levelName = mapName,
                        displayName = mapName,
                        description = $"关卡: {mapName}",
                        nextLevels = new List<string>(),
                        isStartLevel = false
                    };
                    configData.levels.Add(newLevel);
                    addedCount++;
                }

            if (addedCount > 0)
            {
                Debug.Log($"添加了 {addedCount} 个新关卡");
                EditorUtility.DisplayDialog("操作完成", $"成功添加了 {addedCount} 个新关卡", "确定");
            }
            else
            {
                EditorUtility.DisplayDialog("操作完成", "没有新关卡需要添加", "确定");
            }
        }

        [HorizontalGroup("快速操作/Row1")]
        [Button("清理无效关卡")]
        private void CleanupInvalidLevels()
        {
            var removedCount = 0;

            // 移除不存在的关卡
            for (var i = configData.levels.Count - 1; i >= 0; i--)
                if (!availableMaps.Contains(configData.levels[i].levelName))
                {
                    Debug.Log($"移除无效关卡: {configData.levels[i].levelName}");
                    configData.levels.RemoveAt(i);
                    removedCount++;
                }

            // 清理无效的后继关卡引用
            foreach (var level in configData.levels)
                for (var i = level.nextLevels.Count - 1; i >= 0; i--)
                    if (!availableMaps.Contains(level.nextLevels[i]))
                    {
                        Debug.Log($"从关卡 {level.levelName} 中移除无效后继关卡: {level.nextLevels[i]}");
                        level.nextLevels.RemoveAt(i);
                        removedCount++;
                    }

            // 检查默认起始关卡是否有效
            if (!string.IsNullOrEmpty(configData.settings.defaultStartLevel) &&
                !availableMaps.Contains(configData.settings.defaultStartLevel))
            {
                Debug.Log($"清理无效的默认起始关卡: {configData.settings.defaultStartLevel}");
                configData.settings.defaultStartLevel = "";
                removedCount++;
            }

            if (removedCount > 0)
            {
                Debug.Log($"清理了 {removedCount} 个无效引用");
                EditorUtility.DisplayDialog("清理完成", $"成功清理了 {removedCount} 个无效引用", "确定");
            }
            else
            {
                EditorUtility.DisplayDialog("清理完成", "没有发现无效引用", "确定");
            }
        }

        [HorizontalGroup("快速操作/Row2")]
        [Button("验证配置")]
        private void ValidateConfiguration()
        {
            var issues = new List<string>();

            // 检查是否有重复的关卡名称
            var duplicateNames = configData.levels
                .GroupBy(l => l.levelName)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            foreach (var duplicateName in duplicateNames) issues.Add($"发现重复的关卡名称: {duplicateName}");

            // 检查是否有起始关卡
            if (!configData.levels.Any(l => l.isStartLevel)) issues.Add("没有设置任何起始关卡");

            // 检查默认起始关卡是否存在
            if (!string.IsNullOrEmpty(configData.settings.defaultStartLevel))
                if (!configData.levels.Any(l => l.levelName == configData.settings.defaultStartLevel))
                    issues.Add($"默认起始关卡不存在: {configData.settings.defaultStartLevel}");

            // 检查后继关卡是否存在
            foreach (var level in configData.levels)
            foreach (var nextLevel in level.nextLevels)
                if (!configData.levels.Any(l => l.levelName == nextLevel))
                    issues.Add($"关卡 {level.levelName} 的后继关卡不存在: {nextLevel}");

            // 显示验证结果
            if (issues.Count == 0)
            {
                EditorUtility.DisplayDialog("验证通过", "配置验证通过，没有发现问题！", "确定");
            }
            else
            {
                var issueText = string.Join("\n", issues);
                EditorUtility.DisplayDialog("验证失败", $"发现以下问题：\n\n{issueText}", "确定");
                Debug.LogWarning($"配置验证失败:\n{issueText}");
            }
        }

        [HorizontalGroup("快速操作/Row2")]
        [Button("重置配置")]
        private void ResetConfiguration()
        {
            if (EditorUtility.DisplayDialog("确认重置", "确定要重置所有配置吗？这将清空当前所有设置。", "确定", "取消"))
            {
                configData = new LevelConfigData();
                Debug.Log("配置已重置");
            }
        }

        [TitleGroup("商店配置")]
        [HorizontalGroup("商店配置/Row1")]
        [Button("全部设为进入商店")]
        private void SetAllLevelsEnterShop()
        {
            if (EditorUtility.DisplayDialog("确认设置", "确定要将所有关卡设为通关后进入商店吗？", "确定", "取消"))
            {
                foreach (var level in configData.levels) level.shouldEnterShop = true;
                Debug.Log("已将所有关卡设为通关后进入商店");
            }
        }

        [HorizontalGroup("商店配置/Row1")]
        [Button("全部设为不进入商店")]
        private void SetAllLevelsNotEnterShop()
        {
            if (EditorUtility.DisplayDialog("确认设置", "确定要将所有关卡设为通关后不进入商店吗？", "确定", "取消"))
            {
                foreach (var level in configData.levels) level.shouldEnterShop = false;
                Debug.Log("已将所有关卡设为通关后不进入商店");
            }
        }

        [HorizontalGroup("商店配置/Row2")]
        [Button("设置末尾关卡进入商店")]
        private void SetEndLevelsEnterShop()
        {
            var count = 0;
            foreach (var level in configData.levels)
                // 末尾关卡定义：没有后继关卡的关卡
                if (level.nextLevels == null || level.nextLevels.Count == 0)
                {
                    level.shouldEnterShop = true;
                    count++;
                }

            if (count > 0)
            {
                Debug.Log($"已将 {count} 个末尾关卡设为通关后进入商店");
                EditorUtility.DisplayDialog("设置完成", $"成功设置了 {count} 个末尾关卡进入商店", "确定");
            }
            else
            {
                EditorUtility.DisplayDialog("设置完成", "没有找到末尾关卡", "确定");
            }
        }

        [HorizontalGroup("商店配置/Row2")]
        [Button("清除所有商店设置")]
        private void ClearAllShopSettings()
        {
            if (EditorUtility.DisplayDialog("确认清除", "确定要清除所有关卡的商店设置吗？", "确定", "取消"))
            {
                foreach (var level in configData.levels) level.shouldEnterShop = false;
                Debug.Log("已清除所有关卡的商店设置");
            }
        }

        // 配置数据结构
        [Serializable]
        public class LevelConfigData
        {
            [LabelText("关卡列表")]
            [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "GetLevelDisplayName")]
            public List<LevelData> levels = new();

            [LabelText("设置")] [InlineProperty] public LevelSettings settings = new();
        }

        [Serializable]
        public class LevelData
        {
            [LabelText("关卡名称")] [ValueDropdown("GetAvailableMapNames")]
            public string levelName;

            [LabelText("显示名称")] public string displayName;

            [LabelText("描述")] [TextArea(2, 3)] public string description;

            [LabelText("后继关卡")] [ValueDropdown("GetAvailableMapNames")]
            public List<string> nextLevels = new();

            [LabelText("是否为起始关卡")] public bool isStartLevel;

            [LabelText("通关后进入商店")] [Tooltip("勾选后，完成此关卡将进入商店界面")]
            public bool shouldEnterShop;

            [LabelText("是否为最终关卡")] [Tooltip("勾选后，本关卡通关将不显示奖励，直接弹出结算面板并判定为胜利")]
            public bool isFinalLevel;

            // 用于在列表中显示的名称
            public string GetLevelDisplayName()
            {
                return string.IsNullOrEmpty(displayName) ? levelName : $"{levelName} ({displayName})";
            }

            // 获取可用地图名称的下拉列表
            private IEnumerable<string> GetAvailableMapNames()
            {
                var window = GetWindow<LevelConfigEditor>();
                if (window != null) return window.availableMaps;
                return new string[0];
            }
        }

        [Serializable]
        public class LevelSettings
        {
            [LabelText("默认起始关卡")] [ValueDropdown("GetAvailableMapNames")]
            public string defaultStartLevel;

            // 获取可用地图名称的下拉列表
            private IEnumerable<string> GetAvailableMapNames()
            {
                var window = GetWindow<LevelConfigEditor>();
                if (window != null) return window.availableMaps;
                return new string[0];
            }
        }
    }
}
#endif