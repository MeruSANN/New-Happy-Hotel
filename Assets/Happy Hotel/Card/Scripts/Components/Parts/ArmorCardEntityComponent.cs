using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.EntityComponent;
using HappyHotel.Core.ValueProcessing.Components;
using UnityEngine;
using IEventListener = HappyHotel.Core.EntityComponent.IEventListener;

namespace HappyHotel.Card.Components.Parts
{
    // 护甲卡牌组件，负责处理护甲添加逻辑
    public class ArmorCardEntityComponent : EntityComponentBase, IEventListener
    {
        public int ArmorAmount { get; private set; } = 5;

        // 实现IEventListener接口，处理UseCard事件
        public void OnEvent(EntityComponentEvent evt)
        {
            if (evt.EventName == "UseCard") ExecuteArmorLogic();
        }

        public void SetArmorAmount(int value)
        {
            ArmorAmount = Mathf.Max(0, value);
        }

        // 执行护甲添加逻辑
        public bool ExecuteArmorLogic()
        {
            // 查找主角色
            var mainCharacter = GameObject.FindGameObjectWithTag("MainCharacter");
            if (mainCharacter == null)
            {
                Debug.LogError("ArmorCardEntityComponent: 未找到主角色");
                return false;
            }

            // 获取主角色的BehaviorComponentContainer
            var behaviorContainer = mainCharacter.GetComponent<BehaviorComponentContainer>();
            if (behaviorContainer == null)
            {
                Debug.LogError("ArmorCardEntityComponent: 主角色没有BehaviorComponentContainer组件");
                return false;
            }

            // 获取或添加ArmorValueComponent
            var armorComponent = behaviorContainer.GetBehaviorComponent<ArmorValueComponent>();
            if (armorComponent == null)
            {
                // 确保有生命值组件（ArmorValueComponent的依赖）
                var hitPointComponent = behaviorContainer.GetBehaviorComponent<HitPointValueComponent>();
                if (hitPointComponent == null)
                    hitPointComponent = behaviorContainer.AddBehaviorComponent<HitPointValueComponent>();

                // 如果主角色没有护甲组件，添加一个
                armorComponent = behaviorContainer.AddBehaviorComponent<ArmorValueComponent>();

                // 确保ArmorValueComponent正确初始化
                if (armorComponent.ArmorValue == null)
                {
                    Debug.LogWarning("ArmorCardEntityComponent: ArmorValueComponent未正确初始化，尝试重新初始化");
                    armorComponent.OnAttach(behaviorContainer);
                }
            }

            // 添加护甲值
            if (armorComponent.ArmorValue != null)
            {
                armorComponent.AddArmor(ArmorAmount);
                Debug.Log($"ArmorCardEntityComponent: 给主角色添加了 {ArmorAmount} 点护甲");
                return true;
            }

            Debug.LogError("ArmorCardEntityComponent: ArmorValueComponent的ArmorValue为null，无法添加护甲");
            return false;
        }
    }
}