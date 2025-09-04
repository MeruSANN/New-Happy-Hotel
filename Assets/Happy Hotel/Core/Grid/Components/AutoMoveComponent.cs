using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core;
using HappyHotel.GameManager;
using UnityEngine;
using UnityEngine.Events;

namespace HappyHotel.Core.Grid.Components
{
    [DependsOnComponent(typeof(DirectionComponent))]
    [DependsOnComponent(typeof(GridObjectComponent))]
    public class AutoMoveComponent : BehaviorComponentBase
    {
        // 当前网格位置
        private Vector2Int currentPosition;

        // 方向组件
        private DirectionComponent directionComponent;

        // 网格对象接口
        private GridObjectComponent gridObject;

        // 移动事件
        public UnityEvent onMoved = new();

        public override void OnAttach(BehaviorComponentContainer container)
        {
            base.OnAttach(container);

            gridObject = container.GetBehaviorComponent<GridObjectComponent>();
            // 获取网格对象接口
            if (gridObject == null)
            {
                Debug.LogError($"{container.name} 没有 GridObjectComponent 组件!");
                IsEnabled = false;
                return;
            }

            // 获取方向组件
            directionComponent = container.GetBehaviorComponent<DirectionComponent>();
            if (directionComponent == null)
            {
                Debug.LogError($"{container.name} 没有 DirectionComponent 组件!");
                IsEnabled = false;
                return;
            }

            // 订阅时钟系统的时钟信号
            if (ClockSystem.Instance != null) ClockSystem.Instance.onClockTick.AddListener(OnClockTick);
        }

        public override void OnUpdate()
        {
            // 不再需要在这里处理移动逻辑，改为响应时钟信号
        }

        // 响应时钟系统的时钟信号
        private void OnClockTick()
        {
            if (!IsEnabled || gridObject == null || directionComponent == null) return;

            // 获取当前位置
            currentPosition = gridObject.GetGridPosition();

            // 计算下一个目标位置
            var targetPosition = currentPosition + directionComponent.GetDirectionVector();

            // 使用网格管理器检查并执行移动
            if (GridObjectManager.Instance.MoveObject(host, targetPosition))
            {
                // 触发移动事件
                onMoved?.Invoke();
            }
            else
            {
                // 撞到墙壁时反转方向
                directionComponent.Reverse();

                // 通知游戏状态控制器碰到墙体
                GameManager.GameManager.Instance.OnHitWall();
            }
        }

        // 外部请求在当前格子上停止移动（例如踩到阻止前进的Prop）
        public void StopByExternalRequest()
        {
            if (directionComponent == null) return;

            // 撞到墙壁时反转方向
            directionComponent.Reverse();

            // 通知游戏状态控制器碰到墙体
            GameManager.GameManager.Instance.OnHitWall();
        }

        // 外部请求在当前格子上停止移动，并设置停止后的面朝方向
        public void StopByExternalRequest(Direction faceDirection)
        {
            if (directionComponent == null) return;

            // 设置为指定方向
            directionComponent.SetDirection(faceDirection);

            // 通知游戏状态控制器碰到墙体
            GameManager.GameManager.Instance.OnHitWall();
        }

        // 设置时钟系统的时钟间隔（通过时钟系统统一控制）
        public void SetMoveInterval(float interval)
        {
            if (ClockSystem.Instance != null) ClockSystem.Instance.SetClockInterval(interval);
        }

        // 获取时钟系统的时钟间隔
        public float GetMoveInterval()
        {
            if (ClockSystem.Instance != null) return ClockSystem.Instance.GetClockInterval();
            return 0.5f; // 默认值
        }

        // 组件从宿主移除时取消订阅时钟信号
        public override void OnDetach()
        {
            base.OnDetach();

            // 取消订阅时钟系统的时钟信号
            if (ClockSystem.Instance != null) ClockSystem.Instance.onClockTick.RemoveListener(OnClockTick);
        }
    }
}