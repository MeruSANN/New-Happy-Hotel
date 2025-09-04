using System.Collections.Generic;
using System.IO;
using System.Linq;
using HappyHotel.Core.Grid;
using TMPro;
using UnityEngine;

namespace HappyHotel.Map
{
    public class MapStorageUIController : MonoBehaviour
    {
        [Header("UI组件")] public TMP_InputField mapNameInputField;

        public TMP_Dropdown mapListDropdown;
        // 商店设置已迁移到LevelConfig.json中，不再需要UI Toggle

        private MapStorageManager mapStorageManager;

        private void Start()
        {
            // 如果没有手动分配MapStorageManager，尝试自动查找
            if (!mapStorageManager)
            {
                mapStorageManager = FindObjectOfType<MapStorageManager>();
                if (!mapStorageManager)
                {
                    Debug.LogError("MapUIController: 找不到MapStorageManager组件！");
                    return;
                }
            }

            // 初始化时刷新地图列表
            RefreshMapList();
        }

        // 刷新地图列表到Dropdown
        public void RefreshMapList()
        {
            if (!mapStorageManager || !mapListDropdown)
            {
                Debug.LogError("MapUIController: MapStorageManager或Dropdown引用丢失！");
                return;
            }

            // 获取所有可用的地图
            var availableMaps = mapStorageManager.GetAvailableMaps();

            // 清空现有选项
            mapListDropdown.ClearOptions();

            // 添加新选项
            mapListDropdown.AddOptions(availableMaps.Length > 0
                ? availableMaps.ToList()
                // 如果没有地图文件，添加一个提示选项
                : new List<string> { "No map available" });

            Debug.Log($"已刷新地图列表，找到 {availableMaps.Length} 个地图文件");
        }

        // 加载选中的地图（由Button调用）
        public void LoadSelectedMap()
        {
            if (!mapStorageManager || !mapListDropdown || !mapNameInputField)
            {
                Debug.LogError("MapUIController: 必要的组件引用丢失！");
                return;
            }

            // 获取当前选中的地图名
            if (mapListDropdown.options.Count > 0 && mapListDropdown.value >= 0)
            {
                var selectedMapName = mapListDropdown.options[mapListDropdown.value].text;

                // 检查是否为提示文本
                if (selectedMapName == "No map available")
                {
                    Debug.LogWarning("没有可加载的地图文件！");
                    return;
                }

                // 更新InputField为选中的地图名
                mapNameInputField.text = selectedMapName;

                // 加载地图
                var loadSuccess = mapStorageManager.LoadMap(selectedMapName);

                if (loadSuccess)
                {
                    Debug.Log($"成功加载地图: {selectedMapName}");
                    // 加载地图后同步波次管理信息并刷新场景显示
                    if (MapWaveEditManager.Instance != null)
                    {
                        MapWaveEditManager.Instance.InitializeFromMap(mapStorageManager.GetCurrentMapData());
                        MapWaveEditManager.Instance.OverwriteSceneEnemiesFromCurrentWave();
                    }
                }
                else
                    Debug.LogError($"加载地图失败: {selectedMapName}");
            }
            else
            {
                Debug.LogWarning("Dropdown中没有可选择的地图！");
            }
        }

        // 商店设置已迁移到LevelConfig.json中，不再需要从地图数据更新UI

        // 保存当前地图（由Button调用）
        public void SaveCurrentMap()
        {
            if (!mapStorageManager || !mapNameInputField)
            {
                Debug.LogError("MapUIController: 必要的组件引用丢失！");
                return;
            }

            // 获取InputField中的地图名
            var mapName = mapNameInputField.text.Trim();

            // 验证地图名是否有效
            if (string.IsNullOrEmpty(mapName))
            {
                Debug.LogWarning("请输入有效的地图名称！");
                return;
            }

            // 检查文件名是否包含非法字符
            if (mapName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                Debug.LogWarning("地图名称包含非法字符，请使用有效的文件名！");
                return;
            }

            // 保存地图
            mapStorageManager.SaveMap(mapName);

            // 保存后刷新地图列表
            RefreshMapList();

            // 在Dropdown中选中刚保存的地图
            SelectMapInDropdown(mapName);

            Debug.Log($"地图已保存: {mapName}");
        }

        // 在Dropdown中选中指定的地图
        private void SelectMapInDropdown(string mapName)
        {
            if (!mapListDropdown) return;

            for (var i = 0; i < mapListDropdown.options.Count; i++)
                if (mapListDropdown.options[i].text == mapName)
                {
                    mapListDropdown.value = i;
                    break;
                }
        }

        // 当Dropdown选择改变时调用（可选，用于实时更新InputField）
        public void OnDropdownValueChanged()
        {
            if (!mapListDropdown || !mapNameInputField) return;

            if (mapListDropdown.options.Count > 0 && mapListDropdown.value >= 0)
            {
                var selectedMapName = mapListDropdown.options[mapListDropdown.value].text;

                // 只有当选中的不是提示文本时才更新InputField
                if (selectedMapName != "No map available") mapNameInputField.text = selectedMapName;
            }
        }

        // 新建地图（由Button调用）
        public void CreateNewMap()
        {
            if (!mapStorageManager || !mapNameInputField)
            {
                Debug.LogError("MapUIController: 必要的组件引用丢失！");
                return;
            }

            // 清空输入框
            mapNameInputField.text = "";

            // 清空当前地图中的所有元素
            if (MapManager.Instance)
            {
                // 清空所有网格对象（角色、道具、敌人等）
                if (GridObjectManager.Instance) GridObjectManager.Instance.ClearAllObjects();

                // 将所有Tile设为Empty
                var mapSize = MapManager.Instance.GetMapSize();
                for (var x = 0; x < mapSize.x; x++)
                for (var y = 0; y < mapSize.y; y++)
                    MapManager.Instance.SetTile(x, y, new TileInfo(TileType.Empty));

                // 更新视觉地图
                MapManager.Instance.UpdateVisualMap();

                Debug.Log("已创建新地图，清空了所有元素");
            }
            else
            {
                Debug.LogError("MapManager.Instance为空，无法创建新地图！");
            }
        }

        // 商店设置已迁移到LevelConfig.json中，请使用关卡配置编辑器进行设置
    }
}