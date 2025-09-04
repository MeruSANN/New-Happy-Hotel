using HappyHotel.Core;
using HappyHotel.Prop;
using HappyHotel.Prop.Settings;
using UnityEngine;

namespace HappyHotel.Card
{
    // 向左单向方向改变卡牌：放置一个固定向左的方向改变器
    public class DirectionChangerLeftCard : ActivePlacementCard
    {
        public override PropBase PlaceProp(Vector2Int position, IPropSetting setting = null)
        {
            if (template == null)
            {
                Debug.LogError("DirectionChangerLeftCard的模板为空");
                return null;
            }

            var propTypeId = Core.Registry.TypeId.Create<PropTypeId>("DirectionChanger");

            var propController = PropController.Instance;
            if (propController == null)
            {
                Debug.LogError("PropController未初始化");
                return null;
            }

            var directionalSetting = new DirectionalSetting(Direction.Left);
            var prop = propController.PlaceProp(position, propTypeId, directionalSetting);

            if (prop != null)
                Debug.Log($"成功放置向左方向改变器到位置: {position}");
            else
                Debug.LogError($"无法放置向左方向改变器到位置: {position}");

            return prop;
        }
    }
}

