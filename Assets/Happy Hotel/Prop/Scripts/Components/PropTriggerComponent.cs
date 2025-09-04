using System;
using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.Grid.Components;
using UnityEngine;

namespace HappyHotel.Prop.Components
{
    [DependsOnComponent(typeof(GridObjectComponent))]
    public class PropTriggerComponent : BehaviorComponentBase
    {
        private GridObjectComponent gridObject;

        // BeforePropTrigger事件 - 使用C#事件
        public event Action<PropBase> onBeforePropTrigger;

        // AfterPropTrigger事件 - 使用C#事件
        public event Action<PropBase> onAfterPropTrigger;

        // BehaviorComponentBase接口实现
        public override void OnAttach(BehaviorComponentContainer container)
        {
            base.OnAttach(container);

            // 在组件被附加到容器时调用
            gridObject = container.GetBehaviorComponent<GridObjectComponent>();

            if (gridObject != null)
                // 监听onObjectEnter事件
                gridObject.onObjectEnter.AddListener(OnObjectEnter);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            if (gridObject != null)
                // 取消监听onObjectEnter事件
                gridObject.onObjectEnter.RemoveListener(OnObjectEnter);
        }

        // 当其他对象进入同一网格时调用
        private void OnObjectEnter(BehaviorComponentContainer enteringObject)
        {
            if (host == null || enteringObject == null)
                return;

            // 检查进入的对象是否为道具
            var prop = enteringObject as PropBase;
            if (prop != null)
            {
                // 触发BeforePropTrigger事件
                onBeforePropTrigger?.Invoke(prop);

                // 触发道具（PropBase内部会检查初始化状态）
                prop.OnTrigger(host);
                Debug.Log($"PropTriggerComponent触发了道具 {prop.name}");

                // 触发AfterPropTrigger事件
                onAfterPropTrigger?.Invoke(prop);
            }
        }
    }
}