using System;
using UnityEngine;

namespace HappyHotel.Map
{
    // 地格类型枚举
    public enum TileType
    {
        Floor, // 地板，可通行
        Empty, // 空地，可通行
        Wall // 墙体，不可通行
    }

    // 地图上的单个地格对象
    [Serializable]
    public class TileInfo
    {
        [SerializeField] private TileType type = TileType.Empty;

        public TileInfo(TileType type)
        {
            this.type = type;
        }

        public TileType Type
        {
            get => type;
            set => type = value;
        }
    }
}