using System;
using System.Collections.Generic;
using HappyHotel.Core;
using UnityEngine;

namespace HappyHotel.Equipment.Templates
{
    // 方向放置卡牌模板，用于配置需要方向设置的卡牌
    [CreateAssetMenu(fileName = "New Directional Placement Card Template",
        menuName = "Happy Hotel/Item/Directional Placement Card Template")]
    public class DirectionalPlacementCardTemplate : ActivePlacementCardTemplate
    {
        [Header("方向设置")] [Tooltip("不同方向的指示器图片")]
        public Sprite[] directionSprites;

        [Header("可选择方向")] [Tooltip("设置卡牌可以选择的方向，支持多选")]
        public DirectionFlags allowedDirections = DirectionFlags.All;
    }

    // 方向标志枚举，支持多方向选择
    [Flags]
    public enum DirectionFlags
    {
        None = 0,
        Up = 1 << 0,
        Down = 1 << 1,
        Left = 1 << 2,
        Right = 1 << 3,
        All = Up | Down | Left | Right
    }

    // 方向标志扩展方法
    public static class DirectionFlagsExtensions
    {
        // 检查是否包含指定方向
        public static bool HasDirection(this DirectionFlags flags, Direction direction)
        {
            var directionFlag = DirectionToFlag(direction);
            return (flags & directionFlag) != 0;
        }

        // 将Direction转换为DirectionFlags
        public static DirectionFlags DirectionToFlag(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up: return DirectionFlags.Up;
                case Direction.Down: return DirectionFlags.Down;
                case Direction.Left: return DirectionFlags.Left;
                case Direction.Right: return DirectionFlags.Right;
                default: return DirectionFlags.None;
            }
        }

        // 获取所有允许的方向
        public static Direction[] GetAllowedDirections(this DirectionFlags flags)
        {
            var directions = new List<Direction>();

            if (flags.HasDirection(Direction.Up))
                directions.Add(Direction.Up);
            if (flags.HasDirection(Direction.Down))
                directions.Add(Direction.Down);
            if (flags.HasDirection(Direction.Left))
                directions.Add(Direction.Left);
            if (flags.HasDirection(Direction.Right))
                directions.Add(Direction.Right);

            return directions.ToArray();
        }
    }
}