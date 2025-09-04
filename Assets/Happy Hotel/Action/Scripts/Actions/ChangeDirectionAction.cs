using HappyHotel.Core;
using HappyHotel.Core.Grid.Components;
using UnityEngine;

namespace HappyHotel.Action
{
    public class ChangeDirectionAction : ActionBase
    {
        private Direction direction;

        public void SetDirection(Direction direction)
        {
            this.direction = direction;
        }

        public override void Execute()
        {
            // 获取当前行动队列的宿主对象
            var host = actionQueue?.GetHost();
            if (host)
            {
                var directionComponent = host.GetBehaviorComponent<DirectionComponent>();
                if (directionComponent != null)
                {
                    directionComponent.SetDirection(direction);
                    Debug.Log($"改变方向为: {direction}");
                }
                else
                {
                    Debug.LogError("宿主对象没有DirectionComponent组件!");
                }
            }
            else
            {
                Debug.LogError("未找到行动队列宿主对象!");
            }
        }

        // 占位符格式化：{direction}
        protected override string FormatDescriptionInternal(string formattedDescription)
        {
            return formattedDescription.Replace("{direction}", direction.ToString());
        }
    }
}