using UnityEngine;

namespace HappyHotel.UI.HoverDisplay
{
    // 悬停显示主控制器，管理所有悬停显示逻辑
    public class HoverDisplayController : MonoBehaviour
    {
        [Header("悬停显示设置")] [SerializeField] private HoverDisplayUI defaultUI;

        [SerializeField] private bool followMouse = true;
        [SerializeField] private Vector2 offset = new(10f, 10f);

        [Header("显示延迟设置")] [SerializeField] private float displayDelay = 0.3f;

        [SerializeField] private bool showImmediately;
        private HoverDisplayData currentData;

        private HoverDisplayUI currentUI;
        private bool isDisplaying;

        private void Update()
        {
            if (isDisplaying && followMouse && currentUI != null)
            {
                // 更新UI位置跟随鼠标
                Vector2 mousePos = Input.mousePosition;
                var isLeftSide = IsLeftHalf(mousePos);
                ApplySidePivot(isLeftSide);
                var sideAwareOffset = GetSideAwareOffset(isLeftSide);
                currentUI.UpdatePosition(mousePos + sideAwareOffset);
            }
        }

        // 显示悬停UI
        public void ShowHoverUI(HoverDisplayData data, HoverDisplayUI specificUI = null)
        {
            if (data == null) return;

            // 确定要使用的UI
            var targetUI = specificUI ?? defaultUI;
            if (targetUI == null)
            {
                Debug.LogWarning("未指定悬停显示UI");
                return;
            }

            // 如果当前正在显示，先隐藏
            if (isDisplaying) HideHoverUI();

            currentUI = targetUI;
            currentData = data;
            isDisplaying = true;

            Debug.Log(
                $"[HoverDisplayController] ShowHoverUI: UI={currentUI?.GetType().Name}, Data={data?.GetType().Name}");
            // 显示UI
            currentUI.ShowHoverInfo(data);

            // 设置初始位置
            if (followMouse)
            {
                var isLeftSide = IsLeftHalf(data.mousePosition);
                ApplySidePivot(isLeftSide);
                var sideAwareOffset = GetSideAwareOffset(isLeftSide);
                currentUI.UpdatePosition(data.mousePosition + sideAwareOffset);
            }
            else
            {
                // 如果不跟随鼠标，使用数据中的位置
                currentUI.UpdatePosition(data.mousePosition);
            }
        }

        // 隐藏悬停UI
        public void HideHoverUI()
        {
            if (currentUI != null)
            {
                Debug.Log("[HoverDisplayController] HideHoverUI");
                currentUI.HideHoverInfo();
            }

            currentUI = null;
            currentData = null;
            isDisplaying = false;
        }

        // 设置默认UI
        public void SetDefaultUI(HoverDisplayUI ui)
        {
            defaultUI = ui;
        }

        // 设置是否跟随鼠标
        public void SetFollowMouse(bool follow)
        {
            followMouse = follow;
        }

        // 设置偏移量
        public void SetOffset(Vector2 newOffset)
        {
            offset = newOffset;
        }

        // 判断是否在屏幕左半侧
        private bool IsLeftHalf(Vector2 mouseScreenPos)
        {
            return mouseScreenPos.x < Screen.width * 0.5f;
        }

        // 根据左右侧设置Pivot：
        // 左侧 → 悬浮窗在鼠标右侧：pivot 设为左上 (0,1)
        // 右侧 → 悬浮窗在鼠标左侧：pivot 设为右上 (1,1)
        private void ApplySidePivot(bool isLeftSide)
        {
            if (currentUI == null) return;
            var desiredPivot = isLeftSide ? new Vector2(0f, 1f) : new Vector2(1f, 1f);
            currentUI.SetPivot(desiredPivot);
        }

        // 根据左右侧返回偏移：
        // 左侧 → 右下偏移 (offset.x正, offset.y正)
        // 右侧 → 左下偏移 (offset.x负, offset.y正)
        private Vector2 GetSideAwareOffset(bool isLeftSide)
        {
            return new Vector2(Mathf.Abs(offset.x) * (isLeftSide ? 1f : -1f), Mathf.Abs(offset.y));
        }

        // 获取当前显示状态
        public bool IsDisplaying()
        {
            return isDisplaying;
        }

        // 获取当前数据
        public HoverDisplayData GetCurrentData()
        {
            return currentData;
        }

        // 获取显示延迟
        public float GetDisplayDelay()
        {
            return displayDelay;
        }

        // 获取是否立即显示
        public bool GetShowImmediately()
        {
            return showImmediately;
        }

        // 设置显示延迟
        public void SetDisplayDelay(float delay)
        {
            displayDelay = delay;
        }

        // 设置是否立即显示
        public void SetShowImmediately(bool immediately)
        {
            showImmediately = immediately;
        }
    }
}