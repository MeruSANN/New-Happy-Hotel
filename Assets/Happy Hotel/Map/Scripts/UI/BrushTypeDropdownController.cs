using System.Collections.Generic;
using System.Linq;
using HappyHotel.Device;
using HappyHotel.Enemy;
using TMPro;
using UnityEngine;

namespace HappyHotel.Map.UI
{
    // 笔刷类型下拉菜单控制器
    public class BrushTypeDropdownController : MonoBehaviour
    {
        [Header("UI组件")] [SerializeField] private TMP_Dropdown typeDropdown;

        // 当前模式 (0=砖块, 1=装置, 2=敌人, 3=角色, 4=删除)
        private int currentMode;
        private List<string> deviceOptions;
        private List<string> enemyOptions;

        // 缓存的选项数据
        private List<string> tileOptions;

        private void Start()
        {
            InitializeOptions();
            UpdateDropdownForMode(currentMode);
            SetupEventListeners();
        }

        private void OnDestroy()
        {
            if (typeDropdown) typeDropdown.onValueChanged.RemoveListener(OnTypeChanged);
        }

        private void InitializeOptions()
        {
            // 初始化砖块选项
            tileOptions = new List<string> { "Floor", "Wall", "Empty" };

            // 初始化装置选项 - 从DeviceRegistry获取
            deviceOptions = new List<string>();
            if (DeviceRegistry.Instance != null)
            {
                var deviceDescriptors = DeviceRegistry.Instance.GetAllDescriptors();
                deviceOptions = deviceDescriptors.Select(d => d.Type.Id).ToList();

                if (deviceOptions.Count == 0) deviceOptions.Add("无可用装置");
            }
            else
            {
                deviceOptions.Add("DeviceRegistry未初始化");
            }

            // 初始化敌人选项 - 从EnemyRegistry获取
            enemyOptions = new List<string>();
            if (EnemyRegistry.Instance != null)
            {
                var enemyDescriptors = EnemyRegistry.Instance.GetAllDescriptors();
                enemyOptions = enemyDescriptors.Select(e => e.Type.Id).ToList();

                if (enemyOptions.Count == 0) enemyOptions.Add("无可用敌人");
            }
            else
            {
                enemyOptions.Add("EnemyRegistry未初始化");
            }

            Debug.Log(
                $"BrushTypeDropdownController: 初始化完成 - 砖块:{tileOptions.Count}, 装置:{deviceOptions.Count}, 敌人:{enemyOptions.Count}");
        }

        private void SetupEventListeners()
        {
            if (typeDropdown) typeDropdown.onValueChanged.AddListener(OnTypeChanged);
        }

        // 当笔刷模式改变时调用
        public void OnBrushModeChanged(int modeIndex)
        {
            currentMode = modeIndex;
            UpdateDropdownForMode(modeIndex);
        }

        private void UpdateDropdownForMode(int modeIndex)
        {
            if (!typeDropdown)
            {
                Debug.LogError("BrushTypeDropdownController: 未分配TMP_Dropdown组件!");
                return;
            }

            // 清空现有选项
            typeDropdown.ClearOptions();

            var options = new List<string>();

            switch (modeIndex)
            {
                case 0: // 砖块模式
                    options = new List<string>(tileOptions);
                    break;

                case 1: // 装置模式
                    options = new List<string>(deviceOptions);
                    break;

                case 2: // 敌人模式
                    options = new List<string>(enemyOptions);
                    break;

                case 3: // 角色模式
                    // 角色模式不需要显示任何选项
                    options = new List<string>();
                    break;

                case 4: // 删除模式
                    // 删除模式不需要显示任何选项
                    options = new List<string>();
                    break;

                default:
                    options.Add("未知模式");
                    break;
            }

            // 添加选项到下拉菜单
            typeDropdown.AddOptions(options);

            // 设置默认选择
            typeDropdown.value = 0;

            // 立即应用第一个选项
            OnTypeChanged(0);

            Debug.Log($"BrushTypeDropdownController: 更新为模式 {modeIndex}, 选项数量: {options.Count}");
        }

        private void OnTypeChanged(int selectedIndex)
        {
            if (!MapEditBrush.Instance)
            {
                Debug.LogWarning("BrushTypeDropdownController: MapEditBrush实例未找到");
                return;
            }


            typeDropdown.interactable = currentMode != 3 && currentMode != 4;

            switch (currentMode)
            {
                case 0: // 砖块模式
                    HandleTileSelection(selectedIndex);
                    break;

                case 1: // 装置模式
                    HandleDeviceSelection(selectedIndex);
                    break;

                case 2: // 敌人模式
                    HandleEnemySelection(selectedIndex);
                    break;

                case 3: // 角色模式
                    break;

                case 4: // 删除模式
                    // 删除模式不需要处理类型选择
                    break;
            }
        }

        private void HandleTileSelection(int selectedIndex)
        {
            if (selectedIndex < 0 || selectedIndex >= tileOptions.Count) return;

            TileType tileType;
            switch (selectedIndex)
            {
                case 0: // 地板
                    tileType = TileType.Floor;
                    break;
                case 1: // 墙体
                    tileType = TileType.Wall;
                    break;
                case 2: // 空地
                    tileType = TileType.Empty;
                    break;
                default:
                    tileType = TileType.Floor;
                    break;
            }

            MapEditBrush.Instance.SetTileType(tileType);
            Debug.Log($"设置砖块类型为: {tileType}");
        }

        private void HandleDeviceSelection(int selectedIndex)
        {
            if (selectedIndex < 0 || selectedIndex >= deviceOptions.Count) return;

            var deviceType = deviceOptions[selectedIndex];

            // 检查是否是有效的装置类型
            if (deviceType == "无可用装置" || deviceType == "DeviceRegistry未初始化")
            {
                Debug.LogWarning($"无效的装置类型: {deviceType}");
                return;
            }

            MapEditBrush.Instance.SetDeviceType(deviceType);
            Debug.Log($"设置装置类型为: {deviceType}");
        }

        private void HandleEnemySelection(int selectedIndex)
        {
            if (selectedIndex < 0 || selectedIndex >= enemyOptions.Count) return;

            var enemyType = enemyOptions[selectedIndex];

            // 检查是否是有效的敌人类型
            if (enemyType == "无可用敌人" || enemyType == "EnemyRegistry未初始化")
            {
                Debug.LogWarning($"无效的敌人类型: {enemyType}");
                return;
            }

            MapEditBrush.Instance.SetEnemyType(enemyType);
            Debug.Log($"设置敌人类型为: {enemyType}");
        }

        // 刷新选项列表（当Registry内容发生变化时调用）
        public void RefreshOptions()
        {
            InitializeOptions();
            UpdateDropdownForMode(currentMode);
        }

        // 获取当前选择的类型字符串
        public string GetCurrentTypeString()
        {
            if (!typeDropdown || typeDropdown.value < 0) return "";

            switch (currentMode)
            {
                case 0: // 砖块模式
                    return typeDropdown.value < tileOptions.Count ? tileOptions[typeDropdown.value] : "";
                case 1: // 装置模式
                    return typeDropdown.value < deviceOptions.Count ? deviceOptions[typeDropdown.value] : "";
                case 2: // 敌人模式
                    return typeDropdown.value < enemyOptions.Count ? enemyOptions[typeDropdown.value] : "";
                case 3: // 角色模式
                    return ""; // 角色模式没有类型选项
                case 4: // 删除模式
                    return ""; // 删除模式没有类型选项
                default:
                    return "";
            }
        }
    }
}