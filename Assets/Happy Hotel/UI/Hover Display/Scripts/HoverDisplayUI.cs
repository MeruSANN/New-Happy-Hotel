using UnityEngine;

namespace HappyHotel.UI.HoverDisplay
{
    // 悬停显示UI基类，提供显示和隐藏悬停信息的基础功能
    public abstract class HoverDisplayUI : MonoBehaviour
    {
        [Header("悬停显示设置")] [SerializeField] protected CanvasGroup canvasGroup;

        [SerializeField] protected RectTransform rectTransform;
        protected HoverDisplayController controller;
        protected HoverDisplayData currentData;

        protected bool isVisible;

        protected virtual void Awake()
        {
            // 自动获取组件
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();

            // 查找控制器
            controller = FindObjectOfType<HoverDisplayController>();

            // 初始隐藏
            SetVisible(false);
        }

        // 显示悬停信息
        public virtual void ShowHoverInfo(HoverDisplayData data)
        {
            if (data == null) return;

            currentData = data;
            UpdateDisplay(data);
            SetVisible(true);
        }

        // 隐藏悬停信息
        public virtual void HideHoverInfo()
        {
            SetVisible(false);
            currentData = null;
        }

        // 更新显示内容（子类重写）
        protected abstract void UpdateDisplay(HoverDisplayData data);

        // 设置UI可见性（瞬间显示和隐藏）
        protected virtual void SetVisible(bool visible)
        {
            if (isVisible == visible) return;

            isVisible = visible;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.interactable = visible;
                canvasGroup.blocksRaycasts = visible;
            }
            else
            {
                // 如果没有CanvasGroup，才使用SetActive
                gameObject.SetActive(visible);
            }
        }

        // 更新UI位置（跟随鼠标或悬停对象）
        public virtual void UpdatePosition(Vector2 screenPosition)
        {
            UpdatePosition(screenPosition, null);
        }

        // 设置Pivot供控制器动态切换位置锚点
        public void SetPivot(Vector2 newPivot)
        {
            if (rectTransform == null) return;
            rectTransform.pivot = newPivot;
        }

        // 更新UI位置（指定Canvas）
        public virtual void UpdatePosition(Vector2 screenPosition, Canvas targetCanvas)
        {
            if (rectTransform == null) return;

            // 获取Canvas
            var canvas = targetCanvas ?? GetComponentInParent<Canvas>();
            if (canvas == null) return;

            // 根据Canvas的渲染模式处理位置
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                // Screen Space-Camera模式：需要将屏幕坐标转换为Canvas坐标
                Vector2 localPosition;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.transform as RectTransform,
                    screenPosition,
                    canvas.worldCamera,
                    out localPosition);

                rectTransform.localPosition = localPosition;
            }
            else if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                // Screen Space-Overlay模式：直接使用屏幕坐标
                var position = screenPosition;
                var size = rectTransform.sizeDelta;
                var screenSize = new Vector2(Screen.width, Screen.height);

                // 检查右边界
                if (position.x + size.x > screenSize.x)
                    position.x = screenSize.x - size.x;

                // 检查下边界
                if (position.y + size.y > screenSize.y)
                    position.y = screenSize.y - size.y;

                // 确保不超出左边界和上边界
                position.x = Mathf.Max(0, position.x);
                position.y = Mathf.Max(0, position.y);

                rectTransform.position = position;
            }
            else
            {
                // World Space模式：直接设置世界坐标
                rectTransform.position = screenPosition;
            }
        }
    }
}