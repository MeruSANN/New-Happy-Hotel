using UnityEngine;
using UnityEngine.EventSystems;

namespace HappyHotel.UI.HoverDisplay
{
    // 悬停显示接收器，用于检测鼠标悬停事件并通知控制器
    public class HoverDisplayReceiver : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("悬停显示设置")] [SerializeField] protected HoverDisplayUI targetUI;

        [SerializeField] private float displayDelay = 0.5f;
        [SerializeField] private bool showImmediately;
        private HoverDisplayController controller;
        private object customData;
        private bool hasShown; // 防止重复显示
        private float hoverStartTime;

        private bool isHovering;

        private void Awake()
        {
            // 自动获取控制器
            controller = FindObjectOfType<HoverDisplayController>();
            if (controller == null) Debug.LogWarning("未找到HoverDisplayController，悬停显示功能将无法正常工作");
        }

        private void Update()
        {
            if (isHovering && !showImmediately)
                // 检查是否达到显示延迟时间
                if (Time.time - hoverStartTime >= displayDelay)
                    if (!hasShown)
                    {
                        ShowHoverUI();
                        hasShown = true;
                    }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovering = true;
            hoverStartTime = Time.time;
            hasShown = false;

            if (showImmediately)
                if (!hasShown)
                {
                    ShowHoverUI();
                    hasShown = true;
                }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovering = false;
            hasShown = false;
            HideHoverUI();
        }

        // 显示悬停UI
        private void ShowHoverUI()
        {
            if (controller != null)
            {
                var data = CreateHoverData();
                if (data == null)
                {
                    Debug.LogWarning("CreateHoverData 返回 null，无法显示悬停UI");
                    return;
                }

                controller.ShowHoverUI(data, targetUI);
            }
        }

        // 隐藏悬停UI
        private void HideHoverUI()
        {
            if (controller != null) controller.HideHoverUI();
        }

        // 设置自定义数据
        public void SetCustomData(object data)
        {
            customData = data;
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

        // 子类可覆盖以自定义悬停数据的创建
        protected virtual HoverDisplayData CreateHoverData()
        {
            var worldPosition = transform.position;
            Vector2 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
            return new HoverDisplayData(worldPosition, screenPosition)
            {
                customData = customData,
                displayDelay = displayDelay,
                showImmediately = showImmediately
            };
        }
    }
}