using HappyHotel.Prop;
using HappyHotel.Prop.Settings;
using UnityEngine;

namespace HappyHotel.Card
{
    // 跳跃卡牌：在指定位置放置JumpProp
    public class JumpCard : ActivePlacementCard
    {
        public override PropBase PlaceProp(Vector2Int position, IPropSetting setting = null)
        {
            if (template == null)
            {
                Debug.LogError("JumpCard的模板为空");
                return null;
            }

            var propTypeIdString = TypeId.Id;
            var propTypeId = Core.Registry.TypeId.Create<PropTypeId>(propTypeIdString);

            var propController = PropController.Instance;
            if (propController == null)
            {
                Debug.LogError("PropController未初始化");
                return null;
            }

            var prop = propController.PlaceProp(position, propTypeId, setting);

            if (prop != null)
                Debug.Log($"成功放置跳跃道具到位置: {position}");
            else
                Debug.LogError($"无法放置跳跃道具到位置: {position}");

            return prop;
        }
    }
}

