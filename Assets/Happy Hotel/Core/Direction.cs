using UnityEngine;

namespace HappyHotel.Core
{
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    public static class DirectionExtensions
    {
        public static Vector2Int ToVector2Int(this Direction direction)
        {
            switch (direction)
            {
                case Direction.Up: return Vector2Int.up;
                case Direction.Down: return Vector2Int.down;
                case Direction.Left: return Vector2Int.left;
                case Direction.Right: return Vector2Int.right;
                default: return Vector2Int.zero;
            }
        }

        public static Vector3 ToVector3(this Direction direction)
        {
            switch (direction)
            {
                case Direction.Up: return Vector3.up;
                case Direction.Down: return Vector3.down;
                case Direction.Left: return Vector3.left;
                case Direction.Right: return Vector3.right;
                default: return Vector3.zero;
            }
        }

        public static Direction GetOpposite(this Direction direction)
        {
            switch (direction)
            {
                case Direction.Up: return Direction.Down;
                case Direction.Down: return Direction.Up;
                case Direction.Left: return Direction.Right;
                case Direction.Right: return Direction.Left;
                default: return direction;
            }
        }
    }
}