using System;
using System.Collections;
using HappyHotel.Core;
using UnityEngine;
using UnityEngine.UI;

namespace HappyHotel.UI
{
    // 四向选择器UI控制脚本
    // 用于选择道具放置的方向（上、下、左、右）
    public class DirectionSelectorUI : MonoBehaviour
    {
        [Header("方向按钮")] [SerializeField] private Button upButton; // 上方向按钮

        [SerializeField] private Button downButton; // 下方向按钮
        [SerializeField] private Button leftButton; // 左方向按钮
        [SerializeField] private Button rightButton; // 右方向按钮

        [Header("UI设置")] [SerializeField] private GameObject selectorPanel; // 选择器面板

        [SerializeField] private float showAnimationDuration = 0.2f; // 显示动画时长

        [Header("按钮视觉设置")] [SerializeField] private Color enabledButtonColor = Color.white; // 可选择按钮的颜色

        [SerializeField] private Color disabledButtonColor = Color.gray; // 不可选择按钮的颜色
        [SerializeField] private float disabledButtonAlpha = 0.5f; // 不可选择按钮的透明度

        // 是否正在显示

        // 方向选择完成回调
        private Action<Direction> onDirectionSelected;

        // 检查选择器是否正在显示
        public bool IsShowing { get; private set; }

        private void Awake()
        {
            // 初始化按钮事件
            SetupButtons();

            // 初始时隐藏选择器
            if (selectorPanel != null) selectorPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            // 清理按钮事件
            CleanupButtons();
        }

        // 设置按钮事件
        private void SetupButtons()
        {
            if (upButton != null)
                upButton.onClick.AddListener(() => OnDirectionButtonClick(Direction.Up));

            if (downButton != null)
                downButton.onClick.AddListener(() => OnDirectionButtonClick(Direction.Down));

            if (leftButton != null)
                leftButton.onClick.AddListener(() => OnDirectionButtonClick(Direction.Left));

            if (rightButton != null)
                rightButton.onClick.AddListener(() => OnDirectionButtonClick(Direction.Right));
        }

        // 清理按钮事件
        private void CleanupButtons()
        {
            if (upButton != null)
                upButton.onClick.RemoveAllListeners();

            if (downButton != null)
                downButton.onClick.RemoveAllListeners();

            if (leftButton != null)
                leftButton.onClick.RemoveAllListeners();

            if (rightButton != null)
                rightButton.onClick.RemoveAllListeners();
        }

        // 显示方向选择器
        public void Show(Vector3 screenPosition, Action<Direction> callback)
        {
            if (IsShowing)
            {
                Debug.LogWarning("方向选择器已经在显示中");
                return;
            }

            // 确保DirectionSelectorUI所在的GameObject是活跃的，以便能够启动协程
            if (!gameObject.activeInHierarchy) gameObject.SetActive(true);

            onDirectionSelected = callback;
            IsShowing = true;

            // 设置选择器位置
            SetSelectorPosition(screenPosition);

            // 显示选择器面板
            if (selectorPanel != null)
            {
                selectorPanel.SetActive(true);

                // 播放显示动画
                PlayShowAnimation();
            }

            Debug.Log($"显示方向选择器在屏幕位置: {screenPosition}");
        }

        // 隐藏方向选择器
        public void Hide()
        {
            if (!IsShowing) return;

            IsShowing = false;
            onDirectionSelected = null;

            // 隐藏选择器面板
            if (selectorPanel != null) selectorPanel.SetActive(false);

            // 将DirectionSelectorUI所在的GameObject设为非活跃状态以节省性能
            gameObject.SetActive(false);

            Debug.Log("隐藏方向选择器");
        }

        // 设置选择器位置
        private void SetSelectorPosition(Vector3 screenPosition)
        {
            if (selectorPanel == null) return;

            // 设置UI位置
            var rectTransform = selectorPanel.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                // 获取Canvas的Camera引用
                var canvas = rectTransform.GetComponentInParent<Canvas>();
                var canvasCamera = canvas?.worldCamera;

                // 将屏幕位置转换为UI位置
                Vector2 uiPosition;
                var success = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rectTransform.parent as RectTransform,
                    screenPosition,
                    canvasCamera, // 传入Canvas的Camera引用
                    out uiPosition);

                if (success)
                {
                    rectTransform.localPosition = uiPosition;
                    Debug.Log($"方向选择器位置设置成功 - 屏幕坐标: {screenPosition}, UI坐标: {uiPosition}");
                }
                else
                {
                    Debug.LogError($"方向选择器位置设置失败 - 屏幕坐标: {screenPosition}, Canvas Camera: {canvasCamera}");
                }
            }
        }

        // 播放显示动画
        private void PlayShowAnimation()
        {
            if (selectorPanel == null) return;

            // 简单的缩放动画
            var panelTransform = selectorPanel.transform;
            panelTransform.localScale = Vector3.zero;

            // 使用协程播放动画
            StartCoroutine(ScaleAnimation(panelTransform, Vector3.zero, Vector3.one, showAnimationDuration));
        }

        // 缩放动画协程
        private IEnumerator ScaleAnimation(Transform target, Vector3 startScale, Vector3 endScale, float duration)
        {
            var elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                var progress = elapsedTime / duration;

                // 使用缓动函数使动画更平滑
                progress = Mathf.SmoothStep(0f, 1f, progress);

                target.localScale = Vector3.Lerp(startScale, endScale, progress);
                yield return null;
            }

            target.localScale = endScale;
        }

        // 方向按钮点击处理
        private void OnDirectionButtonClick(Direction direction)
        {
            if (!IsShowing)
            {
                Debug.LogWarning("方向选择器未显示，忽略点击");
                return;
            }

            Debug.Log($"方向选择器：用户点击了方向按钮 {direction}");

            // 触发回调
            if (onDirectionSelected != null)
            {
                Debug.Log($"方向选择器：调用回调函数，传递方向 {direction}");
                onDirectionSelected.Invoke(direction);
            }
            else
            {
                Debug.LogWarning("方向选择器：回调函数为null");
            }

            // 隐藏选择器
            Hide();
        }

        // 获取支持的方向列表
        public Direction[] GetSupportedDirections()
        {
            return new[] { Direction.Up, Direction.Down, Direction.Left, Direction.Right };
        }

        // 设置特定方向按钮的可用状态
        public void SetDirectionButtonEnabled(Direction direction, bool enabled)
        {
            var button = GetDirectionButton(direction);
            if (button != null)
            {
                button.interactable = enabled;
                // 同时更新按钮的视觉状态
                UpdateButtonVisualState(button, enabled);
            }
        }

        // 更新按钮的视觉状态（颜色和透明度）
        private void UpdateButtonVisualState(Button button, bool enabled)
        {
            if (button == null) return;

            // 获取按钮的Image组件
            var buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                var targetColor = enabled ? enabledButtonColor : disabledButtonColor;

                // 如果按钮不可用，设置透明度
                if (!enabled)
                    targetColor.a = disabledButtonAlpha;
                else
                    targetColor.a = 1f; // 完全可见

                buttonImage.color = targetColor;
            }
        }

        // 根据可选择的方向数组设置按钮状态
        public void SetAvailableDirections(Direction[] availableDirections)
        {
            // 获取所有支持的方向
            var allDirections = GetSupportedDirections();

            // 遍历所有方向，设置按钮状态
            foreach (var direction in allDirections)
            {
                var isAvailable = Array.Exists(availableDirections, d => d == direction);
                SetDirectionButtonEnabled(direction, isAvailable);
            }
        }

        // 获取指定方向的按钮
        private Button GetDirectionButton(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up: return upButton;
                case Direction.Down: return downButton;
                case Direction.Left: return leftButton;
                case Direction.Right: return rightButton;
                default: return null;
            }
        }

        // 重置所有按钮状态
        public void ResetButtonStates()
        {
            SetDirectionButtonEnabled(Direction.Up, true);
            SetDirectionButtonEnabled(Direction.Down, true);
            SetDirectionButtonEnabled(Direction.Left, true);
            SetDirectionButtonEnabled(Direction.Right, true);
        }
    }
}