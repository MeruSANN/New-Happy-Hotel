using HappyHotel.Prop;
using UnityEngine;

namespace HappyHotel.UI.HoverDisplay.PropHover
{
    // Prop悬停接收器，专门用于检测Prop对象的悬停事件
    public class PropHoverReceiver : HoverDisplayReceiver
    {
        private PropBase propComponent;
        private TextHoverDisplayUI typedTargetUI;

        private void Start()
        {
            propComponent = GetComponent<PropBase>();
            typedTargetUI = targetUI as TextHoverDisplayUI;
            if (typedTargetUI == null)
            {
                typedTargetUI = FindObjectOfType<TextHoverDisplayUI>();
                if (typedTargetUI != null) targetUI = typedTargetUI;
            }

            if (propComponent == null) Debug.LogWarning($"GameObject {gameObject.name} 上没有找到PropBase组件，悬停功能可能无法正常工作");
        }

        protected override HoverDisplayData CreateHoverData()
        {
            if (propComponent == null) return null;
            var worldPosition = transform.position;
            Vector2 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
            var data = new PropHoverData(propComponent, worldPosition, screenPosition);
            Debug.Log($"[PropHoverReceiver] CreateHoverData: 描述长度={data.formattedDescription?.Length}");
            return data;
        }
    }
}