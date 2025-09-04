using System;
using HappyHotel.Core.BehaviorComponent;
using UnityEngine;

namespace HappyHotel.Core.Grid.Components
{
    // 处理实体方向的行为组件
    public class DirectionComponent : BehaviorComponentBase
    {
        private readonly Color arrowColor = Color.red;
        private readonly float arrowLength = 1.0f;
        private readonly bool showDirectionGizmos = true;
        private Direction currentDirection = Direction.Right;

        // 方向改变事件
        public event Action<Direction> onDirectionChanged;

        // 初始化方向组件
        public void Initialize(Direction initialDirection)
        {
            currentDirection = initialDirection;
        }

        // 设置方向
        public void SetDirection(Direction direction)
        {
            if (currentDirection != direction)
            {
                currentDirection = direction;
                onDirectionChanged?.Invoke(currentDirection);
            }
        }

        // 获取当前方向
        public Direction GetDirection()
        {
            return currentDirection;
        }

        // 获取当前方向对应的Vector2Int向量
        public Vector2Int GetDirectionVector()
        {
            return currentDirection.ToVector2Int();
        }

        // 反转当前方向
        public void Reverse()
        {
            SetDirection(currentDirection.GetOpposite());
        }

        // 绘制方向Gizmos
        public override void OnDrawGizmos()
        {
            if (!showDirectionGizmos) return;

            // 设置Gizmos颜色
            Gizmos.color = arrowColor;

            // 获取当前对象的位置
            var position = host.transform.position;

            // 根据当前方向计算箭头终点
            var directionVector = currentDirection.ToVector3();
            var arrowEnd = position + directionVector * arrowLength;

            // 绘制主箭头线
            Gizmos.DrawLine(position, arrowEnd);

            // 绘制箭头头部
            DrawArrowHead(position, arrowEnd, directionVector);
        }

        // 绘制箭头头部
        private void DrawArrowHead(Vector3 start, Vector3 end, Vector3 direction)
        {
            var headLength = arrowLength * 0.3f;

            // 计算箭头头部的两个分支
            var right = Vector3.zero;
            var left = Vector3.zero;

            if (direction == Vector3.up || direction == Vector3.down)
            {
                // 垂直方向的箭头
                right = Vector3.right;
                left = Vector3.left;
            }
            else
            {
                // 水平方向的箭头
                right = Vector3.up;
                left = Vector3.down;
            }

            // 计算箭头头部的两个点
            var rightHead = end - direction * headLength + right * headLength * 0.5f;
            var leftHead = end - direction * headLength + left * headLength * 0.5f;

            // 绘制箭头头部
            Gizmos.DrawLine(end, rightHead);
            Gizmos.DrawLine(end, leftHead);
        }
    }
}