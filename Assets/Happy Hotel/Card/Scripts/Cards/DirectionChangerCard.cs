using HappyHotel.Prop;
using HappyHotel.Prop.Settings;
using UnityEngine;

namespace HappyHotel.Card
{
    // 方向改变卡牌，可以向指定位置放置DirectionChangerProp
    public class DirectionChangerCard : DirectionalPlacementCard
    {
        public override PropBase PlaceProp(Vector2Int position, IPropSetting setting = null)
        {
            if (template == null)
            {
                Debug.LogError("DirectionChangerCard的模板为空");
                return null;
            }

            // 确定要创建的Prop类型ID
            var propTypeIdString = TypeId.Id;

            // 创建对应的PropTypeId
            var propTypeId = Core.Registry.TypeId.Create<PropTypeId>(propTypeIdString);

            // 使用PropController放置道具
            var propController = PropController.Instance;
            if (propController == null)
            {
                Debug.LogError("PropController未初始化");
                return null;
            }

            // 使用传入的设置放置道具
            var prop = propController.PlaceProp(position, propTypeId, setting);

            if (prop != null)
                Debug.Log($"成功放置方向改变器道具到位置: {position}");
            else
                Debug.LogError($"无法放置方向改变器道具到位置: {position}");

            return prop;
        }
    }
}