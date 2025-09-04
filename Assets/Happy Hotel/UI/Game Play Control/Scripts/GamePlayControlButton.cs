using HappyHotel.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HappyHotel.UI.GamePlayControl
{
    // 游戏播放控制按钮
    // 根据游戏状态改变按钮的显示和交互状态
    public class GamePlayControlButton : SingletonConnectedUIBase<GameManager.GameManager>
    {
        [Header("UI组件")] [SerializeField] private Button playButton;

        [SerializeField] private Image buttonImage;
        [SerializeField] private Image backgroundImage; // 背景图片
        [SerializeField] private TMP_Text buttonText; // 按钮文本

        [Header("按钮状态配置")] [SerializeField] private Sprite playSprite; // 播放图标

        [SerializeField] private Sprite pauseSprite; // 暂停图标

        [Header("文本配置")] [SerializeField] private string playText = "开始"; // 播放状态文本

        [SerializeField] private string pauseText = "暂停"; // 暂停状态文本

        [Header("颜色配置")] [SerializeField] private Color enabledColor = Color.white;

        [SerializeField] private Color disabledColor = new(0.5f, 0.5f, 0.5f, 0.5f);
        [SerializeField] private Color enabledBackgroundColor = new(1f, 1f, 1f, 1f); // 启用时背景颜色
        [SerializeField] private Color disabledBackgroundColor = new(0.5f, 0.5f, 0.5f, 0.3f); // 禁用时背景颜色

        protected override void OnUIStart()
        {
            // 如果没有手动指定Button组件，自动获取
            if (playButton == null) playButton = GetComponent<Button>();

            // 如果没有手动指定Image组件，自动获取
            if (buttonImage == null) buttonImage = GetComponent<Image>();

            // 如果没有手动指定背景Image组件，尝试从父对象获取
            if (backgroundImage == null) backgroundImage = transform.parent?.GetComponent<Image>();

            // 如果没有手动指定TMP_Text组件，尝试从子对象获取
            if (buttonText == null) buttonText = GetComponentInChildren<TMP_Text>();
        }

        protected override void OnSingletonConnected()
        {
            // 订阅游戏状态变化事件
            GameManager.GameManager.onGameStateChanged += OnGameStateChanged;

            // 设置按钮点击事件
            if (playButton != null) playButton.onClick.AddListener(OnPlayButtonClicked);

            // 初始化按钮状态
            UpdateButtonState(singletonInstance.GetGameState());
        }

        protected override void OnSingletonDisconnected()
        {
            // 取消订阅事件
            GameManager.GameManager.onGameStateChanged -= OnGameStateChanged;

            // 移除按钮点击事件
            if (playButton != null) playButton.onClick.RemoveListener(OnPlayButtonClicked);
        }

        // 处理游戏状态变化
        private void OnGameStateChanged(GameManager.GameManager.GameState newState)
        {
            UpdateButtonState(newState);
        }

        // 处理播放按钮点击
        private void OnPlayButtonClicked()
        {
            if (!IsConnectedToSingleton()) return;

            var currentState = singletonInstance.GetGameState();

            // 只有在静止状态时才能点击开始播放
            if (currentState == GameManager.GameManager.GameState.Idle)
                singletonInstance.SetGameState(GameManager.GameManager.GameState.Playing);
        }

        // 更新按钮状态
        private void UpdateButtonState(GameManager.GameManager.GameState gameState)
        {
            if (playButton == null) return;

            switch (gameState)
            {
                case GameManager.GameManager.GameState.Idle:
                    // 静止状态：按钮可点击，显示播放图标和文本
                    SetButtonEnabled(true);
                    SetButtonAsPlay();
                    break;

                case GameManager.GameManager.GameState.Playing:
                    // 播放状态：按钮不可点击，显示暂停图标和文本
                    SetButtonEnabled(false);
                    SetButtonAsPause();
                    break;

                case GameManager.GameManager.GameState.Reward:
                    // 奖励状态：按钮不可点击，显示暂停图标和文本
                    SetButtonEnabled(false);
                    SetButtonAsPause();
                    break;
            }
        }

        // 设置按钮为播放状态
        private void SetButtonAsPlay()
        {
            if (buttonImage != null && playSprite != null) buttonImage.sprite = playSprite;

            if (buttonText != null) buttonText.text = playText;
        }

        // 设置按钮为暂停状态
        private void SetButtonAsPause()
        {
            if (buttonImage != null && pauseSprite != null) buttonImage.sprite = pauseSprite;

            if (buttonText != null) buttonText.text = pauseText;
        }

        // 设置按钮启用/禁用状态
        private void SetButtonEnabled(bool enabled)
        {
            if (playButton != null) playButton.interactable = enabled;

            // 设置按钮图片颜色
            if (buttonImage != null) buttonImage.color = enabled ? enabledColor : disabledColor;

            // 设置背景图片颜色
            if (backgroundImage != null)
                backgroundImage.color = enabled ? enabledBackgroundColor : disabledBackgroundColor;
        }
    }
}