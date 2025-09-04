using HappyHotel.Core.BehaviorComponent;
using UnityEngine;
using UnityEngine.Events;

namespace HappyHotel.Core.Grid.Components
{
    // 网格对象组件，用于替代IGridObject接口
    public class GridObjectComponent : BehaviorComponentBase
    {
        // 是否可以放在墙体上
        private bool canPlaceOnWalls;

        // 当前网格位置
        private Vector2Int gridPosition;

        // 对象进入和退出事件
        public UnityEvent<BehaviorComponentContainer> onObjectEnter = new();
        public UnityEvent<BehaviorComponentContainer> onObjectExit = new();

        // 初始化方法，用于设置是否可以放在墙体上
        public void Initialize(bool canPlaceOnWalls = false)
        {
            this.canPlaceOnWalls = canPlaceOnWalls;
        }

        // 获取是否可以放在墙体上
        public bool CanPlaceOnWalls()
        {
            return canPlaceOnWalls;
        }

        public override void OnAttach(BehaviorComponentContainer container)
        {
            base.OnAttach(container);

            // 在组件被添加时自动注册到网格管理器
            if (GridObjectManager.Instance != null && Application.isPlaying)
                GridObjectManager.Instance.RegisterObject(host, gridPosition);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            // 在组件被销毁时从网格管理器中注销
            if (GridObjectManager.Instance) GridObjectManager.Instance.UnregisterObject(host);
        }

        public void MoveTo(Vector2Int position)
        {
            if (GridObjectManager.Instance) GridObjectManager.Instance.MoveObject(host, position);
        }

        // 设置对象在网格中的位置
        public void SetVisualGridPosition(Vector2Int position)
        {
            gridPosition = position;
            if (GridObjectManager.Instance) host.transform.position = GridObjectManager.Instance.GridToWorld(position);
        }

        public Vector2Int GetGridPosition()
        {
            return gridPosition;
        }

        // 当其他对象进入同一网格时调用
        public bool HandleObjectEnter(GridObjectComponent other)
        {
            if (other != null && other.host)
            {
                onObjectEnter.Invoke(other.host);
                return true;
            }

            return false;
        }

        // 当其他对象离开同一网格时调用
        public void HandleObjectExit(GridObjectComponent other)
        {
            if (other != null && other.host) onObjectExit.Invoke(other.host);
        }

        // 获取游戏对象
        public GameObject GetGameObject()
        {
            return host.gameObject;
        }
    }
}