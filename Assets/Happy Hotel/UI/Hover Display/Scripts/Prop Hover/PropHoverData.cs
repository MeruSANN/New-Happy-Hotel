using System;
using HappyHotel.Prop;
using UnityEngine;

namespace HappyHotel.UI.HoverDisplay.PropHover
{
    // Prop悬停数据结构，只包含Prop的格式化描述
    [Serializable]
    public class PropHoverData : HoverDisplayData
    {
        // Prop对象
        public PropBase prop;

        // 格式化描述
        public string formattedDescription;

        public PropHoverData(PropBase prop, Vector3 position, Vector2 mousePos) : base(position, mousePos)
        {
            this.prop = prop;

            if (prop != null)
            {
                // 获取格式化描述
                formattedDescription = prop.GetFormattedDescription();
                Debug.Log($"[PropHoverData] 构造: prop={prop.Name}, 描述长度={formattedDescription?.Length}");
            }
        }

        // 更新数据（用于实时更新）
        public void UpdateData()
        {
            if (prop == null) return;

            // 更新格式化描述（可能会变化）
            formattedDescription = prop.GetFormattedDescription();
            Debug.Log($"[PropHoverData] UpdateData: prop={prop.Name}, 新描述长度={formattedDescription?.Length}");
        }
    }
}