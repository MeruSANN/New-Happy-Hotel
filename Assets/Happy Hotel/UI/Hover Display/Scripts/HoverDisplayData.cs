using System;
using UnityEngine;

namespace HappyHotel.UI.HoverDisplay
{
    // 悬停显示数据结构，用于传递悬停对象的信息给UI
    [Serializable]
    public class HoverDisplayData
    {
        // 悬停对象的Transform
        public Transform hoveredTransform;

        // 悬停位置（世界坐标）
        public Vector3 hoverPosition;

        // 鼠标位置（屏幕坐标）
        public Vector2 mousePosition;

        // 显示延迟时间（秒）
        public float displayDelay = 0.5f;

        // 是否立即显示（忽略延迟）
        public bool showImmediately;

        // 自定义数据，由接收器设置
        public object customData;

        public HoverDisplayData(Vector3 position, Vector2 mousePos)
        {
            hoverPosition = position;
            mousePosition = mousePos;
        }
    }
}