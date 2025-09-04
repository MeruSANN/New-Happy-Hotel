using HappyHotel.Prop;
using HappyHotel.Prop.Settings;
using UnityEngine;

namespace HappyHotel.Card
{
    // 幽灵方向改变器卡牌：放置 GhostDirectionChangerProp（可设置方向）
    public class GhostDirectionChangerCard : DirectionalPlacementCard
    {
        public override PropBase PlaceProp(Vector2Int position, IPropSetting setting = null)
        {
            if (template == null)
            {
                Debug.LogError("GhostDirectionChangerCard的模板为空");
                return null;
            }

            var propTypeIdString = TypeId.Id; // TypeId 与工厂注册保持一致
            var propTypeId = Core.Registry.TypeId.Create<PropTypeId>(propTypeIdString);

            var propController = PropController.Instance;
            if (propController == null)
            {
                Debug.LogError("PropController未初始化");
                return null;
            }

            var prop = propController.PlaceProp(position, propTypeId, setting);

            if (prop != null)
                Debug.Log($"成功放置幽灵方向改变器到位置: {position}");
            else
                Debug.LogError($"无法放置幽灵方向改变器到位置: {position}");

            return prop;
        }
    }
}

