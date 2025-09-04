using System.Collections.Generic;
using HappyHotel.Core;
using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.Grid.Components;
using TMPro;
using UnityEngine;

namespace HappyHotel.Map.UI
{
    // 主角色朝向下拉菜单控制器
    public class MainCharacterDirectionDropdownController : MonoBehaviour
    {
        [Header("UI组件")] [SerializeField] private TMP_Dropdown directionDropdown;

        // 方向选项
        private readonly string[] directionOptions = { "up", "down", "left", "right" };
        private DirectionComponent currentDirectionComponent;

        // 当前主角色对象
        private GameObject currentMainCharacter;

        private void Start()
        {
            InitializeDropdown();
            SetupEventListeners();
            UpdateDropdownState();
        }

        private void Update()
        {
            // 每帧检查主角色状态
            CheckMainCharacterStatus();
        }

        private void OnDestroy()
        {
            if (directionDropdown) directionDropdown.onValueChanged.RemoveListener(OnDirectionChanged);
        }

        private void InitializeDropdown()
        {
            if (!directionDropdown)
            {
                Debug.LogError("MainCharacterDirectionDropdownController: 未分配TMP_Dropdown组件!");
                return;
            }

            // 清空现有选项
            directionDropdown.ClearOptions();

            Debug.Log("MainCharacterDirectionDropdownController: 下拉菜单初始化完成");
        }

        private void SetupEventListeners()
        {
            if (directionDropdown) directionDropdown.onValueChanged.AddListener(OnDirectionChanged);
        }

        private void CheckMainCharacterStatus()
        {
            var mainCharacter = GameObject.FindGameObjectWithTag("MainCharacter");

            // 如果主角色状态发生变化
            if (mainCharacter != currentMainCharacter)
            {
                currentMainCharacter = mainCharacter;

                if (currentMainCharacter)
                {
                    // 获取方向组件
                    var behaviorContainer = currentMainCharacter.GetComponent<BehaviorComponentContainer>();
                    if (behaviorContainer)
                        currentDirectionComponent = behaviorContainer.GetBehaviorComponent<DirectionComponent>();
                }
                else
                {
                    currentDirectionComponent = null;
                }

                UpdateDropdownState();
            }

            // 如果有主角色，更新当前选择的方向
            if (currentMainCharacter && currentDirectionComponent != null) UpdateCurrentDirectionSelection();
        }

        private void UpdateDropdownState()
        {
            if (!directionDropdown) return;

            if (currentMainCharacter && currentDirectionComponent != null)
            {
                // 有主角色时，显示方向选项
                directionDropdown.ClearOptions();
                var options = new List<string>(directionOptions);
                directionDropdown.AddOptions(options);

                // 设置当前方向
                var currentDirection = currentDirectionComponent.GetDirection();
                directionDropdown.value = (int)currentDirection;

                // 启用下拉菜单
                directionDropdown.interactable = true;

                Debug.Log($"主角色已添加到地图，当前朝向: {currentDirection}");
            }
            else
            {
                // 没有主角色时，清空选项并禁用
                directionDropdown.ClearOptions();
                directionDropdown.interactable = false;

                Debug.Log("地图中没有主角色，下拉菜单已禁用");
            }
        }

        private void UpdateCurrentDirectionSelection()
        {
            if (!directionDropdown || currentDirectionComponent == null) return;

            var currentDirection = currentDirectionComponent.GetDirection();
            var directionIndex = (int)currentDirection;

            // 只有当选择的值与当前方向不同时才更新，避免触发事件
            if (directionDropdown.value != directionIndex)
            {
                // 临时移除监听器，避免循环触发
                directionDropdown.onValueChanged.RemoveListener(OnDirectionChanged);
                directionDropdown.value = directionIndex;
                directionDropdown.onValueChanged.AddListener(OnDirectionChanged);
            }
        }

        private void OnDirectionChanged(int selectedIndex)
        {
            if (!currentMainCharacter || currentDirectionComponent == null)
            {
                Debug.LogWarning("MainCharacterDirectionDropdownController: 没有可控制的主角色");
                return;
            }

            // 将索引转换为Direction枚举
            var newDirection = (Direction)selectedIndex;

            // 设置主角色朝向
            currentDirectionComponent.SetDirection(newDirection);

            Debug.Log($"主角色朝向已改变为: {newDirection}");
        }

        // 获取当前选择的方向索引
        public int GetCurrentDirectionIndex()
        {
            return directionDropdown ? directionDropdown.value : 0;
        }

        // 程序化设置方向
        public void SetDirection(Direction direction)
        {
            if (currentMainCharacter && currentDirectionComponent != null)
            {
                currentDirectionComponent.SetDirection(direction);

                // 更新下拉菜单显示
                if (directionDropdown) directionDropdown.value = (int)direction;
            }
        }

        // 检查是否有主角色
        public bool HasMainCharacter()
        {
            return currentMainCharacter != null;
        }
    }
}