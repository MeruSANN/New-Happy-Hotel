using HappyHotel.UI.HoverDisplay.BuffHover;
using HappyHotel.UI.HoverDisplay.PropHover;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HappyHotel.UI.HoverDisplay
{
    // 通用文本悬停显示UI，用于显示格式化描述文本
    public class TextHoverDisplayUI : HoverDisplayUI
    {
        [Header("文本显示组件")] [SerializeField] private TextMeshProUGUI descriptionText;

        [Header("Canvas设置")] [SerializeField] private Canvas targetCanvas; // 指定用于位置计算的Canvas

        private ContentSizeFitter contentSizeFitter;

        private string currentDescription;

        protected override void Awake()
        {
            base.Awake();

            // 如果没有指定targetCanvas，尝试找到父级Canvas
            if (targetCanvas == null) targetCanvas = GetComponentInParent<Canvas>();

            // 获取Content Size Fitter组件
            contentSizeFitter = GetComponent<ContentSizeFitter>();
        }

        // 重写UpdatePosition方法，使用指定的Canvas
        public override void UpdatePosition(Vector2 screenPosition)
        {
            UpdatePosition(screenPosition, targetCanvas);
        }

        // 重写ShowHoverInfo方法以正确处理文本数据
        public override void ShowHoverInfo(HoverDisplayData data)
        {
            // 检查数据类型
            if (data is PropHoverData propData)
            {
                Debug.Log(
                    $"[TextHoverDisplayUI] ShowHoverInfo 接收到PropHoverData，描述长度={propData.formattedDescription?.Length}, 目标对象={propData.prop?.Name}");
                currentDescription = propData.formattedDescription;
                UpdateTextDisplay();
                base.ShowHoverInfo(data);
            }
            else if (data is BuffHoverData buffData)
            {
                Debug.Log(
                    $"[TextHoverDisplayUI] ShowHoverInfo 接收到BuffHoverData，描述长度={buffData.formattedDescription?.Length}, Buff={buffData.buffInstance?.GetType().Name} / {buffData.buffTypeId}");
                currentDescription = buffData.formattedDescription;
                UpdateTextDisplay();
                base.ShowHoverInfo(data);
            }
            else
            {
                Debug.LogWarning($"TextHoverDisplayUI接收到不支持的数据类型: {data?.GetType().Name}");
                base.ShowHoverInfo(data);
            }
        }

        // 更新文本显示
        protected override void UpdateDisplay(HoverDisplayData data)
        {
            UpdateTextDisplay();
        }

        // 更新文本显示内容
        private void UpdateTextDisplay()
        {
            if (descriptionText != null)
            {
                Debug.Log($"[TextHoverDisplayUI] UpdateTextDisplay currentDescription=\"{currentDescription}\"");
                if (!string.IsNullOrEmpty(currentDescription))
                {
                    descriptionText.text = currentDescription;
                    descriptionText.gameObject.SetActive(true);
                }
                else
                {
                    descriptionText.text = "无描述";
                    descriptionText.gameObject.SetActive(true);
                }

                // 强制刷新Content Size Fitter
                ForceRefreshContentSizeFitter();
            }
            else
            {
                Debug.LogWarning("[TextHoverDisplayUI] descriptionText 为空，无法更新文本显示");
            }
        }

        // 强制刷新Content Size Fitter
        private void ForceRefreshContentSizeFitter()
        {
            if (contentSizeFitter != null)
                // 强制立即更新布局
                LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        }

        // 隐藏悬停信息
        public override void HideHoverInfo()
        {
            Debug.Log("[TextHoverDisplayUI] HideHoverInfo");
            base.HideHoverInfo();
            currentDescription = null;
        }

        // 设置目标Canvas
        public void SetTargetCanvas(Canvas canvas)
        {
            targetCanvas = canvas;
        }

        // 直接设置描述文本（用于外部调用）
        public void SetDescription(string description)
        {
            currentDescription = description;
            UpdateTextDisplay();
            Debug.Log($"[TextHoverDisplayUI] SetDescription 被调用，长度={description?.Length}");
        }
    }
}