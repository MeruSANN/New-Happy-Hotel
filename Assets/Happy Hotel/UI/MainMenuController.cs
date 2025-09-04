using HappyHotel.Core.Scene;
using HappyHotel.GameManager;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace HappyHotel.UI
{
    // 主菜单控制器
    public class MainMenuController : MonoBehaviour
    {
        [Header("UI按钮")] [SerializeField] private Button newGameButton;

        [SerializeField] private Button exitButton;

        [Header("角色选择UI")] [SerializeField] [Tooltip("角色选择UI控制器")]
        private CharacterSelectionUIController characterSelectionUI;

        [Header("新游戏设置")] [SerializeField] [Tooltip("开始新游戏前是否需要玩家选择角色")]
        private bool enableCharacterSelection = true;

        [SerializeField] [Tooltip("游戏场景名称")] private string gameSceneName = "GameScene";

        [SerializeField] [Tooltip("默认角色配置的Resources路径，如 CharacterSelectionConfigs/DefaultCharacter")]
        private string defaultCharacterConfigPath = "CharacterSelectionConfigs/DefaultCharacter";

        private void Start()
        {
            InitializeButtons();
        }

        private void OnDestroy()
        {
            // 清理事件监听器
            if (newGameButton) newGameButton.onClick.RemoveListener(OnNewGameClicked);
            if (exitButton) exitButton.onClick.RemoveListener(OnExitClicked);
        }

        private void InitializeButtons()
        {
            // 新游戏按钮
            if (newGameButton)
                newGameButton.onClick.AddListener(OnNewGameClicked);
            else
                Debug.LogWarning("MainMenuController: 新游戏按钮未分配");

            // 退出按钮
            if (exitButton) exitButton.onClick.AddListener(OnExitClicked);
        }

        // 新游戏按钮点击处理
        private void OnNewGameClicked()
        {
            Debug.Log("点击新游戏按钮");

            // 根据开关决定是否显示角色选择界面
            if (enableCharacterSelection)
            {
                if (characterSelectionUI != null)
                    characterSelectionUI.Show();
                else
                    Debug.LogError("CharacterSelectionUIController未分配，无法显示角色选择界面");
                return;
            }

            // 不选择角色，直接以默认角色开始游戏
            CharacterSelectionConfig defaultConfig = null;
            if (!string.IsNullOrEmpty(defaultCharacterConfigPath))
            {
                defaultConfig = Resources.Load<CharacterSelectionConfig>(defaultCharacterConfigPath);
                if (defaultConfig == null) Debug.LogError($"无法加载默认角色配置: {defaultCharacterConfigPath}");
            }
            else
            {
                Debug.LogError("默认角色配置路径为空");
            }

            if (SceneTransitionManager.Instance != null)
                SceneTransitionManager.Instance.StartNewGame(gameSceneName, defaultConfig);
            else
                Debug.LogError("SceneTransitionManager实例不存在，无法开始新游戏");
        }

        // 角色选择按钮点击处理
        private void OnCharacterSelectionClicked()
        {
            Debug.Log("点击角色选择按钮");

            // 显示角色选择界面
            if (characterSelectionUI != null)
                characterSelectionUI.Show();
            else
                Debug.LogError("CharacterSelectionUIController未分配，无法显示角色选择界面");
        }

        // 退出按钮点击处理
        private void OnExitClicked()
        {
            Debug.Log("点击退出按钮");

#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}