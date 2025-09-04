#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HappyHotel.Core.Editor;
using HappyHotel.Core.Registry;
using HappyHotel.Enemy;
using HappyHotel.GameManager;
using HappyHotel.Map.Data;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace HappyHotel.Map.Editor
{
    public class MapDataConfigEditor : OdinEditorWindow
    {
        // 地图数据文件夹路径
        private const string MAP_DATA_FOLDER = "Assets/StreamingAssets/Maps";

        [TitleGroup("地图选择")] [ValueDropdown("GetAvailableMaps")] [OnValueChanged("LoadSelectedMap")] [LabelText("选择地图")]
        public string selectedMapName = "";

        [TitleGroup("地图配置")] [ShowIf("@!string.IsNullOrEmpty(selectedMapName)")] [InlineProperty] [HideLabel]
        public MapDataConfig mapConfig = new();

        // 可用地图列表
        private readonly List<string> availableMaps = new();

        // 当前加载的地图数据
        private MapData currentMapData;

        protected override void OnEnable()
        {
            base.OnEnable();
            RefreshAvailableMaps();
            // 启动时刷新（清理）通用注册缓存，确保读取到最新注册
            RegistryTypeIdUtility.ClearCache();
        }

        [MenuItem("Happy Hotel/地图数据配置编辑器")]
        public static void ShowWindow()
        {
            var window = GetWindow<MapDataConfigEditor>();
            window.titleContent = new GUIContent("地图数据配置");
            window.minSize = new Vector2(600, 400);
            window.Show();
        }

        // 刷新可用地图列表
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
        }

        // 获取可用地图列表
        private IEnumerable<string> GetAvailableMaps()
        {
            if (availableMaps.Count == 0) RefreshAvailableMaps();
            return availableMaps;
        }

        // 加载选中的地图
        private void LoadSelectedMap()
        {
            if (string.IsNullOrEmpty(selectedMapName))
            {
                currentMapData = null;
                mapConfig = new MapDataConfig();
                return;
            }

            try
            {
                var filePath = Path.Combine(MAP_DATA_FOLDER, selectedMapName + ".happymap");
                if (!File.Exists(filePath))
                {
                    Debug.LogError($"地图文件不存在: {filePath}");
                    return;
                }

                // 读取地图数据
                var json = File.ReadAllText(filePath);
                currentMapData = JsonUtility.FromJson<MapData>(json);

                // 转换为编辑器格式
                ConvertToEditorFormat();

                Debug.Log($"已加载地图: {selectedMapName}");
            }
            catch (Exception e)
            {
                Debug.LogError($"加载地图失败: {e.Message}");
            }
        }

        // 转换为编辑器格式（移除旧间隔刷新读取，仅保留基本关卡信息）
        private void ConvertToEditorFormat()
        {
            if (currentMapData == null)
            {
                mapConfig = new MapDataConfig();
                return;
            }

            mapConfig = new MapDataConfig();
            mapConfig.levelType = currentMapData.levelType;
            mapConfig.levelDifficulty = currentMapData.levelDifficulty;
        }

        // 转换为地图数据格式（移除旧间隔刷新写回，仅保留基本关卡信息）
        private void ConvertToMapDataFormat()
        {
            if (currentMapData == null) return;

            currentMapData.levelType = mapConfig.levelType;
            currentMapData.levelDifficulty = mapConfig.levelDifficulty;
        }

        [TitleGroup("操作")]
        [HorizontalGroup("操作/Buttons")]
        [Button("刷新地图列表", ButtonSizes.Medium)]
        private void RefreshMaps()
        {
            RefreshAvailableMaps();
            Debug.Log($"发现 {availableMaps.Count} 个地图文件");
        }

        [HorizontalGroup("操作/Buttons")]
        [Button("刷新敌人类型", ButtonSizes.Medium)]
        private void RefreshEnemyTypes()
        {
            RegistryTypeIdUtility.ClearCache();
            var count = RegistryTypeIdUtility.GetRegisteredTypeIdsByRegistrationAttribute<EnemyRegistrationAttribute>().Count();
            Debug.Log($"敌人类型已刷新，共找到 {count} 种类型");
        }

        [HorizontalGroup("操作/Buttons")]
        [Button("保存配置", ButtonSizes.Medium)]
        [EnableIf("@!string.IsNullOrEmpty(selectedMapName) && currentMapData != null")]
        private void SaveConfiguration()
        {
            if (currentMapData == null)
            {
                EditorUtility.DisplayDialog("错误", "没有加载地图数据", "确定");
                return;
            }

            try
            {
                // 转换为地图数据格式
                ConvertToMapDataFormat();

                // 保存到文件
                var filePath = Path.Combine(MAP_DATA_FOLDER, selectedMapName + ".happymap");
                var json = JsonUtility.ToJson(currentMapData, true);
                File.WriteAllText(filePath, json);

                AssetDatabase.Refresh();
                Debug.Log($"地图配置已保存: {selectedMapName}");
                EditorUtility.DisplayDialog("保存成功", $"地图 {selectedMapName} 的配置已成功保存！", "确定");
            }
            catch (Exception e)
            {
                Debug.LogError($"保存配置失败: {e.Message}");
                EditorUtility.DisplayDialog("保存失败", $"保存配置时发生错误：{e.Message}", "确定");
            }
        }

        [HorizontalGroup("操作/Buttons")]
        [Button("重置配置", ButtonSizes.Medium)]
        [EnableIf("@!string.IsNullOrEmpty(selectedMapName)")]
        private void ResetConfiguration()
        {
            if (EditorUtility.DisplayDialog("确认重置", "确定要重置当前地图的配置吗？这将清除所有间隔刷新和金币奖励设置。", "确定", "取消"))
            {
                mapConfig = new MapDataConfig();
                Debug.Log("配置已重置");
            }
        }

        [TitleGroup("预览")]
        [ShowIf("@!string.IsNullOrEmpty(selectedMapName)")]
        [Button("预览配置信息", ButtonSizes.Large)]
        [PropertySpace(SpaceBefore = 10)]
        private void PreviewConfiguration()
        {
            if (mapConfig == null)
            {
                EditorUtility.DisplayDialog("预览", "没有配置数据", "确定");
                return;
            }

            var info = $"地图: {selectedMapName}\n";
            info += $"关卡类型: {mapConfig.levelType}\n";
            info += $"关卡难度: {mapConfig.levelDifficulty}\n";

            var registeredEnemyTypes = new HashSet<string>(RegistryTypeIdUtility.GetRegisteredTypeIdsByRegistrationAttribute<EnemyRegistrationAttribute>());
            info += $"系统中已注册敌人类型: {registeredEnemyTypes.Count} 种\n\n";

            EditorUtility.DisplayDialog("配置预览", info, "确定");
        }

        // 地图配置数据结构（用于编辑界面）
        [Serializable]
        public class MapDataConfig
        {
            [TitleGroup("关卡配置")] [LabelText("关卡类型")] [Tooltip("关卡类型")]
            public LevelType levelType = LevelType.Normal;

            [LabelText("关卡难度")] [Tooltip("关卡难度（1-10）")] [Range(1, 10)]
            public int levelDifficulty = 1;

            // 已移除：间隔敌人刷新配置编辑UI
        }
        // 已移除：旧的间隔刷新配置编辑器与权重编辑器
    }
}
#endif