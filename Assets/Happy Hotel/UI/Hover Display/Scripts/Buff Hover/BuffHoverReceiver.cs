using UnityEngine;

namespace HappyHotel.UI.HoverDisplay.BuffHover
{
    // Buff悬停接收器，挂在 BuffIconDisplayer 上，显示Buff描述
    public class BuffHoverReceiver : HoverDisplayReceiver
    {
        private BuffIconDisplayer iconDisplayer;
        private TextHoverDisplayUI typedTargetUI;

        private void Start()
        {
            iconDisplayer = GetComponent<BuffIconDisplayer>();
            typedTargetUI = targetUI as TextHoverDisplayUI;
            if (typedTargetUI == null)
            {
                typedTargetUI = FindObjectOfType<TextHoverDisplayUI>();
                if (typedTargetUI != null) targetUI = typedTargetUI;
            }

            if (iconDisplayer == null)
                Debug.LogWarning($"GameObject {gameObject.name} 上没有找到BuffIconDisplayer组件，悬停功能可能无法正常工作");
        }

        protected override HoverDisplayData CreateHoverData()
        {
            if (iconDisplayer == null) return null;

            var worldPosition = transform.position;
            var screenPosition = Camera.main != null
                ? (Vector2)Camera.main.WorldToScreenPoint(worldPosition)
                : (Vector2)Input.mousePosition;

            var buff = iconDisplayer.GetCurrentBuff();
            if (buff != null) return new BuffHoverData(buff, worldPosition, screenPosition);

            if (iconDisplayer.TryGetTypeId(out var typeId))
                return new BuffHoverData(typeId, worldPosition, screenPosition);

            return null;
        }
    }
}