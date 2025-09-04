using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Equipment.Templates;

namespace HappyHotel.Prop
{
    // 可主动放置的道具抽象基类
    // 这类道具有对应的主动道具，可以在地图上删除时恢复主动道具
    public abstract class ActivePlaceablePropBase : PropBase
    {
        // 是否在被触发时自动销毁
        protected bool autoDestroyOnTrigger;

        // 当模板被设置时调用
        protected override void OnTemplateSet()
        {
            base.OnTemplateSet();

            // 检查是否为主动放置卡牌模板，如果是则设置autoDestroyOnTrigger
            if (template is ActivePlacementCardTemplate activePlacementCardTemplate)
                autoDestroyOnTrigger = activePlacementCardTemplate.autoDestroyOnTrigger;
            else
                // 非主动放置卡牌默认为不自动销毁
                autoDestroyOnTrigger = false;
        }

        // 重写OnTrigger方法，添加自动销毁逻辑
        public override void OnTrigger(BehaviorComponentContainer triggerer)
        {
            // 调用父类的OnTrigger方法
            base.OnTrigger(triggerer);

            // 检查是否需要自动销毁
            if (autoDestroyOnTrigger) Destroy(gameObject);
        }

        // 获取对应的卡牌TypeId
        public string GetCorrespondingCardTypeId()
        {
            return TypeId?.Id;
        }
    }
}