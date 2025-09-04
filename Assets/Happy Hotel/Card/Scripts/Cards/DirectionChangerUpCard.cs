using HappyHotel.Core;
using HappyHotel.Prop;
using HappyHotel.Prop.Settings;
using UnityEngine;

namespace HappyHotel.Card
{
    // 向上单向方向改变卡牌：放置一个固定向上的方向改变器
    public class DirectionChangerUpCard : ActivePlacementCard
    {
        public override PropBase PlaceProp(Vector2Int position, IPropSetting setting = null)
        {
            if (template == null)
            {
                Debug.LogError("DirectionChangerUpCard的模板为空");
                return null;
            }

            // 使用方向改变器的Prop类型
            var propTypeId = Core.Registry.TypeId.Create<PropTypeId>("DirectionChanger");

            var propController = PropController.Instance;
            if (propController == null)
            {
                Debug.LogError("PropController未初始化");
                return null;
            }

            // 固定方向设置为Up
            var directionalSetting = new DirectionalSetting(Direction.Up);
            var prop = propController.PlaceProp(position, propTypeId, directionalSetting);

            if (prop != null)
                Debug.Log($"成功放置向上方向改变器到位置: {position}");
            else
                Debug.LogError($"无法放置向上方向改变器到位置: {position}");

            return prop;
        }
    }
}

