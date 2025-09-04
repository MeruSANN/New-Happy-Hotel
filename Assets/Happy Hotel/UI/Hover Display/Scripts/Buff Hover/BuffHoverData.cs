using System;
using HappyHotel.Buff;
using UnityEngine;

namespace HappyHotel.UI.HoverDisplay.BuffHover
{
    // Buff悬停数据结构，包含Buff的格式化描述
    [Serializable]
    public class BuffHoverData : HoverDisplayData
    {
        // 格式化描述
        public string formattedDescription;

        // Buff实例（可为空）
        public BuffBase buffInstance;

        // Buff类型ID（用于仅有类型时）
        public BuffTypeId buffTypeId;

        public BuffHoverData(BuffBase buff, Vector3 position, Vector2 mousePos) : base(position, mousePos)
        {
            buffInstance = buff;
            if (buffInstance != null)
            {
                buffTypeId = buffInstance.TypeId;
                formattedDescription = buffInstance.GetFormattedDescription();
            }
        }

        public BuffHoverData(BuffTypeId typeId, Vector3 position, Vector2 mousePos) : base(position, mousePos)
        {
            buffTypeId = typeId;
            // 通过模板直接获取描述模板
            var template = BuffManager.Instance?.GetResourceManager()?.GetTemplate(typeId);
            formattedDescription = template?.description ?? "";
        }

        // 更新数据（若实例存在）
        public void UpdateData()
        {
            if (buffInstance == null) return;
            formattedDescription = buffInstance.GetFormattedDescription();
        }
    }
}