using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace HappyHotel.Map.UI
{
    // 笔刷模式下拉菜单控制器
    public class BrushModeDropdownController : MonoBehaviour
    {
        [Header("UI组件")] [SerializeField] private TMP_Dropdown modeDropdown;

        [Header("关联的类型下拉菜单")] [SerializeField] private BrushTypeDropdownController typeDropdownController;

        // 模式选项
        private readonly string[] modeOptions = { "Tile", "Device", "Enemy", "Character", "Delete" };

        private void Start()
        {
            InitializeDropdown();
            SetupEventListeners();
        }

        private void OnDestroy()
        {
            if (modeDropdown) modeDropdown.onValueChanged.RemoveListener(OnModeChanged);
        }

        private void InitializeDropdown()
        {
            if (!modeDropdown)
            {
                Debug.LogError("BrushModeDropdownController: 未分配TMP_Dropdown组件!");
                return;
            }

            // 清空现有选项
            modeDropdown.ClearOptions();

            // 添加模式选项
            var options = new List<string>(modeOptions);
            modeDropdown.AddOptions(options);

            // 设置默认选择
            modeDropdown.value = 0; // 默认选择砖块模式

            Debug.Log("BrushModeDropdownController: 下拉菜单初始化完成");
        }

        private void SetupEventListeners()
        {
            if (modeDropdown) modeDropdown.onValueChanged.AddListener(OnModeChanged);
        }

        private void OnModeChanged(int selectedIndex)
        {
            if (!MapEditBrush.Instance)
            {
                Debug.LogWarning("BrushModeDropdownController: MapEditBrush实例未找到");
                return;
            }

            switch (selectedIndex)
            {
                case 0: // 砖块模式
                    MapEditBrush.Instance.SetBrushMode(BrushMode.DrawTile);
                    Debug.Log("切换到砖块绘制模式");
                    break;

                case 1: // 装置模式
                    MapEditBrush.Instance.SetBrushMode(BrushMode.PlaceObject);
                    Debug.Log("切换到装置放置模式");
                    break;

                case 2: // 敌人模式
                    MapEditBrush.Instance.SetBrushMode(BrushMode.PlaceObject);
                    Debug.Log("切换到敌人放置模式");
                    break;

                case 3: // 角色模式
                    MapEditBrush.Instance.SetMainCharacterMode();
                    Debug.Log("切换到角色放置模式");
                    break;

                case 4: // 删除模式
                    MapEditBrush.Instance.SetDeleteMode();
                    Debug.Log("切换到删除模式");
                    break;

                default:
                    Debug.LogWarning($"BrushModeDropdownController: 未知的模式索引 {selectedIndex}");
                    break;
            }

            // 通知类型下拉菜单更新
            if (typeDropdownController) typeDropdownController.OnBrushModeChanged(selectedIndex);
        }

        // 获取当前选择的模式索引
        public int GetCurrentModeIndex()
        {
            return modeDropdown ? modeDropdown.value : 0;
        }

        // 程序化设置模式
        public void SetMode(int modeIndex)
        {
            if (modeDropdown && modeIndex >= 0 && modeIndex < modeOptions.Length)
            {
                modeDropdown.value = modeIndex;
                OnModeChanged(modeIndex);
            }
        }
    }
}