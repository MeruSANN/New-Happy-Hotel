using HappyHotel.Action.Components;
using HappyHotel.Action.Components.Parts;
using HappyHotel.Core.EntityComponent;
using HappyHotel.Core.ValueProcessing.Components;
using UnityEngine;

namespace HappyHotel.Action
{
    // 护甲翻倍行动，将当前护甲量翻倍
    [AutoInitEntityComponent(typeof(ArmorEntityComponent))]
    [AutoInitEntityComponent(typeof(ArmorValueTrackerComponent))]
    public class DoubleArmorAction : ActionBase
    {
        private readonly ArmorEntityComponent armorComponent;
        private readonly ArmorValueTrackerComponent trackerComponent;

        public DoubleArmorAction()
        {
            armorComponent = GetEntityComponent<ArmorEntityComponent>();
            trackerComponent = GetEntityComponent<ArmorValueTrackerComponent>();

            // 订阅护甲值改变事件
            if (trackerComponent != null) trackerComponent.onArmorValueChanged += ArmorValueChanged;

            // 不在构造函数中调用UpdateArmorAmount，因为此时trackerComponent还没有初始化
            // 会在SetActionQueue时通过事件系统触发更新
        }

        // 重写GetActionValue方法，返回当前的护甲增加量
        public override int GetActionValue()
        {
            return 0;
        }

        ~DoubleArmorAction()
        {
            if (trackerComponent != null) trackerComponent.onArmorValueChanged -= ArmorValueChanged;
        }

        public override void SetActionQueue(ActionQueueComponent queue)
        {
            base.SetActionQueue(queue);

            // 在SetActionQueue后，ArmorValueTrackerComponent应该已经初始化完成
            // 立即尝试更新护甲增加量
            UpdateArmorAmount();
        }

        // 处理护甲值改变事件
        private void ArmorValueChanged(int newArmorValue)
        {
            UpdateArmorAmount();
        }

        // 根据当前护甲值更新护甲增加量
        private void UpdateArmorAmount()
        {
            if (armorComponent == null || trackerComponent == null)
                return;

            // 如果trackerComponent还没有初始化护甲值，尝试手动获取
            var currentArmor = trackerComponent.CurrentArmorValue;
            if (currentArmor == 0 && actionQueue != null && actionQueue.GetHost() != null)
            {
                var hostBehaviorContainer = actionQueue.GetHost();
                var armorValueComponent = hostBehaviorContainer.GetBehaviorComponent<ArmorValueComponent>();
                if (armorValueComponent != null)
                {
                    currentArmor = armorValueComponent.CurrentArmor;
                    Debug.Log($"护甲翻倍: 手动获取当前护甲值 {currentArmor}");
                }
            }

            // 设置护甲增加量等于当前护甲值，这样执行时会翻倍
            armorComponent.SetArmorAmount(currentArmor);

            // 通知数值变化
            NotifyActionValueChanged(currentArmor);

            Debug.Log($"护甲翻倍: 更新护甲增加量为 {currentArmor} (当前护甲值)");
        }

        // 获取当前护甲增加量
        public int GetArmorAmount()
        {
            return armorComponent?.ArmorAmount ?? 0;
        }

        // 占位符格式化：{armor}
        protected override string FormatDescriptionInternal(string formattedDescription)
        {
            var armor = GetArmorAmount();
            return formattedDescription
                .Replace("{armor}", armor.ToString());
        }
    }
}