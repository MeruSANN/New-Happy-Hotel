using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.Grid.Components;
using UnityEngine;

namespace HappyHotel.Action.Components
{
    // 要求容器必须有GridObjectComponent组件
    [DependsOnComponent(typeof(GridObjectComponent))]
    public class ActionConsumerComponent : BehaviorComponentBase
    {
        private string collisionId;

        // 网格组件引用
        private GridObjectComponent gridComponent;

        // 是否激活状态
        [SerializeField] private bool isActive = true;

        public override void OnAttach(BehaviorComponentContainer host)
        {
            base.OnAttach(host);
            gridComponent = host.GetBehaviorComponent<GridObjectComponent>();
            gridComponent.onObjectEnter.AddListener(OnObjectEnter);
            gridComponent.onObjectExit.AddListener(OnObjectExit);
        }

        public override void OnDetach()
        {
            if (gridComponent != null)
            {
                gridComponent.onObjectEnter.RemoveListener(OnObjectEnter);
                gridComponent.onObjectExit.RemoveListener(OnObjectExit);
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (gridComponent != null)
            {
                gridComponent.onObjectEnter.RemoveListener(OnObjectEnter);
                gridComponent.onObjectExit.RemoveListener(OnObjectExit);
            }
        }

        private void OnObjectEnter(BehaviorComponentContainer other)
        {
            if (!isActive) return;

            // 仅发出请求，由ActionChainResolver决定是否执行
            collisionId = ActionChainResolver.Instance.Request(GetHost(), other);
        }

        private void OnObjectExit(BehaviorComponentContainer other)
        {
            if (!isActive) return;

            if (collisionId != null)
            {
                ActionChainResolver.Instance.ReleaseCollisionLock(collisionId);
                collisionId = null;
            }
        }

        // 设置组件激活状态
        public void SetActive(bool active)
        {
            isActive = active;
        }
    }
}