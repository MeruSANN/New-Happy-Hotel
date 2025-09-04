using System.Collections.Generic;
using HappyHotel.Core.Scene;
using HappyHotel.GameManager;
using UnityEngine;
using UnityEngine.UI;

namespace HappyHotel.UI
{
    public class CharacterSelectionUIController : MonoBehaviour
    {
        [Header("UI组件")] [SerializeField] [Tooltip("角色按钮的父级Transform")]
        private Transform characterButtonParent;

        [SerializeField] [Tooltip("角色按钮预制体")] private Button characterButtonPrefab;

        [SerializeField] [Tooltip("关闭按钮")] private Button closeButton;

        [Header("游戏场景配置")] [SerializeField] [Tooltip("游戏场景名称")]
        private string gameSceneName = "GameScene";

        // 生成的角色按钮列表
        private readonly List<Button> characterButtons = new();

        private void Start()
        {
            InitializeUI();
        }

        private void OnDestroy()
        {
            // 清理事件监听器
            if (closeButton) closeButton.onClick.RemoveListener(OnCloseButtonClicked);

            // 清理角色按钮的事件监听器
            foreach (var button in characterButtons)
                if (button != null)
                    button.onClick.RemoveAllListeners();

            // 清理CharacterButton事件监听器
            CharacterButton.onCharacterSelected -= OnCharacterButtonClicked;
        }

#if UNITY_EDITOR
        // 编辑器验证方法
        private void OnValidate()
        {
            // 验证游戏场景名称不为空
            if (string.IsNullOrEmpty(gameSceneName)) Debug.LogWarning("CharacterSelectionUIController: 游戏场景名称不能为空");

            // 验证角色按钮预制体
            if (characterButtonPrefab != null)
                // 检查预制体是否有Button组件
                if (characterButtonPrefab.GetComponent<Button>() == null)
                    Debug.LogError("CharacterSelectionUIController: 角色按钮预制体必须包含Button组件");
        }
#endif

        // 验证UI配置是否正确
        private bool ValidateUIConfiguration()
        {
            var isValid = true;

            if (characterButtonParent == null)
            {
                Debug.LogError("CharacterSelectionUIController: 角色按钮父级Transform未分配");
                isValid = false;
            }

            if (characterButtonPrefab == null)
            {
                Debug.LogError("CharacterSelectionUIController: 角色按钮预制体未分配");
                isValid = false;
            }

            if (closeButton == null) Debug.LogWarning("CharacterSelectionUIController: 关闭按钮未分配");

            return isValid;
        }

        private void InitializeUI()
        {
            // 验证UI配置
            if (!ValidateUIConfiguration())
            {
                Debug.LogError("CharacterSelectionUIController: UI配置验证失败，无法初始化");
                return;
            }

            // 初始化关闭按钮
            if (closeButton)
                closeButton.onClick.AddListener(OnCloseButtonClicked);
            else
                Debug.LogWarning("CharacterSelectionUIController: 关闭按钮未分配");

            // 生成角色按钮
            GenerateCharacterButtons();
        }

        // 生成角色选择按钮
        private void GenerateCharacterButtons()
        {
            // 清除现有按钮
            ClearCharacterButtons();

            // 检查CharacterSelectionManager是否可用
            if (CharacterSelectionManager.Instance == null)
            {
                Debug.LogError("CharacterSelectionManager实例不存在，无法生成角色按钮");
                return;
            }

            // 获取可用角色列表
            var availableCharacters = CharacterSelectionManager.Instance.GetAvailableCharacters();

            if (availableCharacters == null || availableCharacters.Length == 0)
            {
                Debug.LogWarning("没有可用的角色配置");
                return;
            }

            // 为每个角色生成按钮
            for (var i = 0; i < availableCharacters.Length; i++)
            {
                var character = availableCharacters[i];
                if (character != null && character.IsValid())
                    CreateCharacterButton(character, i);
                else
                    Debug.LogWarning($"跳过无效的角色配置: {character?.name ?? "null"}");
            }

            Debug.Log($"生成了 {characterButtons.Count} 个角色选择按钮");
        }

        // 创建单个角色按钮
        private void CreateCharacterButton(CharacterSelectionConfig character, int index)
        {
            if (characterButtonPrefab == null)
            {
                Debug.LogError("角色按钮预制体未分配");
                return;
            }

            if (characterButtonParent == null)
            {
                Debug.LogError("角色按钮父级Transform未分配");
                return;
            }

            // 实例化按钮
            var button = Instantiate(characterButtonPrefab, characterButtonParent);
            characterButtons.Add(button);

            // 尝试获取CharacterButton组件
            var characterButtonComponent = button.GetComponent<CharacterButton>();
            if (characterButtonComponent != null)
            {
                // 使用CharacterButton组件设置角色配置
                characterButtonComponent.SetCharacterConfig(character);

                // 监听角色选择事件
                CharacterButton.onCharacterSelected += OnCharacterButtonClicked;
            }
            else
            {
                // 如果没有CharacterButton组件，使用传统方式设置按钮文字
                var buttonText = button.GetComponentInChildren<Text>();
                if (buttonText != null) buttonText.text = character.CharacterName;

                // 设置按钮点击事件
                button.onClick.AddListener(() => OnCharacterButtonClicked(character));
            }

            // 设置按钮名称
            button.name = $"CharacterButton_{character.CharacterTypeId}";
        }

        // 角色按钮点击处理
        private void OnCharacterButtonClicked(CharacterSelectionConfig character)
        {
            if (character == null)
            {
                Debug.LogError("尝试选择空角色配置");
                return;
            }

            Debug.Log($"选择角色: {character.CharacterName}");

            // 通过CharacterSelectionManager选择角色
            if (CharacterSelectionManager.Instance != null)
            {
                CharacterSelectionManager.Instance.SelectCharacter(character);
            }
            else
            {
                Debug.LogError("CharacterSelectionManager实例不存在，无法选择角色");
                return;
            }

            // 开始新游戏
            StartNewGameWithCharacter(character);
        }

        // 使用指定角色开始新游戏
        private void StartNewGameWithCharacter(CharacterSelectionConfig character)
        {
            if (SceneTransitionManager.Instance)
                SceneTransitionManager.Instance.StartNewGame(gameSceneName, character);
            else
                Debug.LogError("SceneTransitionManager不存在，无法开始新游戏");
        }

        // 关闭按钮点击处理
        private void OnCloseButtonClicked()
        {
            Debug.Log("关闭角色选择界面");
            gameObject.SetActive(false);
        }

        // 清除所有角色按钮
        private void ClearCharacterButtons()
        {
            foreach (var button in characterButtons)
                if (button != null)
                    DestroyImmediate(button.gameObject);

            characterButtons.Clear();
        }

        // 显示角色选择界面
        public void Show()
        {
            gameObject.SetActive(true);
            // 重新生成按钮（以防角色配置有更新）
            GenerateCharacterButtons();
        }

        // 隐藏角色选择界面
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        // 刷新角色按钮（当角色配置更新时调用）
        public void RefreshCharacterButtons()
        {
            GenerateCharacterButtons();
        }

        // 获取当前生成的角色按钮数量
        public int GetCharacterButtonCount()
        {
            return characterButtons.Count;
        }

        // 检查是否有可用的角色
        public bool HasAvailableCharacters()
        {
            if (CharacterSelectionManager.Instance == null)
                return false;

            var characters = CharacterSelectionManager.Instance.GetAvailableCharacters();
            return characters != null && characters.Length > 0;
        }

        // 获取当前选择的角色
        public CharacterSelectionConfig GetCurrentSelectedCharacter()
        {
            if (CharacterSelectionManager.Instance == null)
                return null;

            return CharacterSelectionManager.Instance.GetSelectedCharacter();
        }
    }
}