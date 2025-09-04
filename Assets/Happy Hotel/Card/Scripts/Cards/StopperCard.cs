using HappyHotel.Core;
using HappyHotel.Prop;
using HappyHotel.Prop.Settings;
using UnityEngine;

namespace HappyHotel.Card
{
    // 停止器卡牌：在指定位置放置StopperProp，并设置其方向
    public class StopperCard : DirectionalPlacementCard
    {
        public override PropBase PlaceProp(Vector2Int position, IPropSetting setting = null)
        {
            if (template == null)
            {
                Debug.LogError("StopperCard的模板为空");
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

            // 如果外部未传入设置，则使用当前所选方向构造DirectionalSetting
            var finalSetting = setting;
            if (finalSetting == null)
            {
                // 默认取模板允许方向中的第一个或Right；通常上层UI应提供具体方向
                var allowed = GetAllowedDirections();
                var chosen = allowed != null && allowed.Length > 0 ? allowed[0] : Direction.Right;
                finalSetting = new DirectionalSetting(chosen);
            }

            var prop = propController.PlaceProp(position, propTypeId, finalSetting);

            if (prop != null)
                Debug.Log($"成功放置停止器道具到位置: {position}");
            else
                Debug.LogError($"无法放置停止器道具到位置: {position}");

            return prop;
        }
    }
}

