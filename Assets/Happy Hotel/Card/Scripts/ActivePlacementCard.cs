using HappyHotel.Equipment.Templates;
using HappyHotel.Inventory;
using HappyHotel.Prop;
using HappyHotel.Prop.Settings;
using UnityEngine;

namespace HappyHotel.Card
{
    // 主动放置道具卡牌，负责向指定位置放置对应的Prop
    public abstract class ActivePlacementCard : CardBase
    {
        // 获取模板的便捷属性
        protected ActivePlacementCardTemplate ActiveTemplate => template as ActivePlacementCardTemplate;

        // 重写UseCard方法，统一使用接口
        public override bool UseCard()
        {
            // 对于主动放置卡牌，需要外部提供位置信息
            // 这里可以抛出异常或返回false，表示需要位置参数
            Debug.LogWarning("ActivePlacementCard需要位置信息，请使用UseCard(Vector2Int position, IPropSetting setting)方法");
            return false;
        }

        // 重载UseCard方法，接受位置参数
        public virtual bool UseCard(Vector2Int position, IPropSetting setting = null)
        {
            // 先调用基类的UseCard方法发送事件
            base.UseCard();

            // 然后执行放置逻辑
            var placedProp = PlaceProp(position, setting);
            if (placedProp != null)
            {
                // 设置道具的卡牌引用
                placedProp.SetPlacedByCard(this);
                placedProp.MarkPlacementByCard();

                // 将卡牌添加到临时区（不从中牌区移除）
                if (CardInventory.Instance != null) CardInventory.Instance.AddToTemporaryZone(this);

                return true;
            }

            return false;
        }

        // 向指定位置放置对应的Prop（由子类实现具体逻辑）
        public abstract PropBase PlaceProp(Vector2Int position, IPropSetting setting = null);
    }
}