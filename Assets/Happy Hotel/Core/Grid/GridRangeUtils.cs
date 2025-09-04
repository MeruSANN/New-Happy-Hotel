using System.Collections.Generic;
using UnityEngine;

namespace HappyHotel.Core.Grid.Utils
{
    // 网格范围计算工具类，提供各种范围计算方法
    public static class GridRangeUtils
    {
        /// <summary>
        ///     获取指定中心点周围指定半径的8向相邻位置（包括斜对角）
        /// </summary>
        /// <param name="center">中心位置</param>
        /// <param name="radius">半径（1表示紧邻的8个格子）</param>
        /// <returns>范围内的所有位置列表</returns>
        public static List<Vector2Int> Get8DirectionRange(Vector2Int center, int radius = 1)
        {
            var positions = new List<Vector2Int>();

            // 遍历以center为中心的正方形区域
            for (var x = -radius; x <= radius; x++)
            for (var y = -radius; y <= radius; y++)
            {
                // 跳过中心点自己
                if (x == 0 && y == 0) continue;

                positions.Add(center + new Vector2Int(x, y));
            }

            return positions;
        }

        /// <summary>
        ///     获取指定中心点周围指定半径的4向相邻位置（上下左右）
        /// </summary>
        /// <param name="center">中心位置</param>
        /// <param name="radius">半径</param>
        /// <returns>范围内的所有位置列表</returns>
        public static List<Vector2Int> Get4DirectionRange(Vector2Int center, int radius = 1)
        {
            var positions = new List<Vector2Int>();

            // 4个方向的单位向量
            Vector2Int[] directions =
            {
                Vector2Int.up, // 上
                Vector2Int.down, // 下
                Vector2Int.left, // 左
                Vector2Int.right // 右
            };

            for (var r = 1; r <= radius; r++)
                foreach (var direction in directions)
                    positions.Add(center + direction * r);

            return positions;
        }

        /// <summary>
        ///     获取指定中心点周围的对角线位置
        /// </summary>
        /// <param name="center">中心位置</param>
        /// <param name="radius">半径</param>
        /// <returns>对角线位置列表</returns>
        public static List<Vector2Int> GetDiagonalRange(Vector2Int center, int radius = 1)
        {
            var positions = new List<Vector2Int>();

            // 4个对角线方向的单位向量
            Vector2Int[] diagonals =
            {
                new(-1, 1), // 左上
                new(1, 1), // 右上
                new(-1, -1), // 左下
                new(1, -1) // 右下
            };

            for (var r = 1; r <= radius; r++)
                foreach (var diagonal in diagonals)
                    positions.Add(center + diagonal * r);

            return positions;
        }

        /// <summary>
        ///     获取曼哈顿距离范围内的所有位置
        /// </summary>
        /// <param name="center">中心位置</param>
        /// <param name="distance">曼哈顿距离</param>
        /// <returns>范围内的所有位置列表</returns>
        public static List<Vector2Int> GetManhattanRange(Vector2Int center, int distance = 1)
        {
            var positions = new List<Vector2Int>();

            for (var x = -distance; x <= distance; x++)
            for (var y = -distance; y <= distance; y++)
            {
                // 跳过中心点自己
                if (x == 0 && y == 0) continue;

                // 检查曼哈顿距离
                if (Mathf.Abs(x) + Mathf.Abs(y) <= distance) positions.Add(center + new Vector2Int(x, y));
            }

            return positions;
        }

        /// <summary>
        ///     获取直线范围内的位置（指定方向）
        /// </summary>
        /// <param name="start">起始位置</param>
        /// <param name="direction">方向向量</param>
        /// <param name="length">长度</param>
        /// <returns>直线范围内的位置列表</returns>
        public static List<Vector2Int> GetLineRange(Vector2Int start, Vector2Int direction, int length)
        {
            var positions = new List<Vector2Int>();

            for (var i = 1; i <= length; i++) positions.Add(start + direction * i);

            return positions;
        }
    }
}