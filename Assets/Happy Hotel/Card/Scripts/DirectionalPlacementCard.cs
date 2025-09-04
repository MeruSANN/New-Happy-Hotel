using HappyHotel.Core;
using HappyHotel.Equipment.Templates;
using HappyHotel.Prop;
using HappyHotel.Prop.Settings;
using UnityEngine;

namespace HappyHotel.Card
{
    // 方向放置卡牌，需要方向设置的放置道具卡牌基类
    public abstract class DirectionalPlacementCard : ActivePlacementCard
    {
        // 获取模板的便捷属性
        protected DirectionalPlacementCardTemplate DirectionalTemplate => template as DirectionalPlacementCardTemplate;

        // 获取可选择的方向
        public Direction[] GetAllowedDirections()
        {
            if (DirectionalTemplate != null) return DirectionalTemplate.allowedDirections.GetAllowedDirections();

            // 如果模板为空，返回所有方向作为默认值
            return new[] { Direction.Up, Direction.Down, Direction.Left, Direction.Right };
        }

        // 检查指定方向是否被允许
        public bool IsDirectionAllowed(Direction direction)
        {
            if (DirectionalTemplate != null) return DirectionalTemplate.allowedDirections.HasDirection(direction);

            // 如果模板为空，默认允许所有方向
            return true;
        }

        // 子类需要实现具体的放置逻辑
        public abstract override PropBase PlaceProp(Vector2Int position, IPropSetting setting = null);
    }
}