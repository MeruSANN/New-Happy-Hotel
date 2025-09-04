using HappyHotel.Core;
using HappyHotel.Core.Grid.Components;
using UnityEngine;

namespace HappyHotel.Prop.Settings
{
    // 通用方向设置类
    // 用于为支持方向的道具设置方向
    public class DirectionalSetting : PropSettingBase
    {
        protected Direction direction;

        public DirectionalSetting(Direction direction)
        {
            this.direction = direction;
        }

        // 获取方向
        public Direction GetDirection()
        {
            return direction;
        }

        // 设置方向
        public void SetDirection(Direction newDirection)
        {
            direction = newDirection;
        }

        protected override void ConfigurePropInternal(PropBase prop)
        {
            // 通过DirectionComponent组件设置方向
            var directionComponent = prop.GetBehaviorComponent<DirectionComponent>();
            if (directionComponent != null)
            {
                directionComponent.SetDirection(direction);

                // 如果是DirectionChangerProp，标记为已被Setting配置
                if (prop is DirectionChangerProp directionChangerProp) directionChangerProp.MarkAsConfiguredBySetting();
            }
            else
            {
                Debug.LogError($"DirectionalSetting: 道具 {prop.name} 没有DirectionComponent组件");
            }
        }
    }
}