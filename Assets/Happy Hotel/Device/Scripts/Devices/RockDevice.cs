using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.Grid.Components;
using UnityEngine;

namespace HappyHotel.Device
{
    public class RockDevice : DeviceBase, IBlockingDevice
    {
        protected override void Awake()
        {
            base.Awake();

            // 监听网格对象进入事件（可选：添加碰撞效果）
            var gridComponent = GetBehaviorComponent<GridObjectComponent>();
            if (gridComponent != null) gridComponent.onObjectEnter.AddListener(OnObjectEnter);
        }

        public bool IsDestructible => false; // 石头不可破坏

        // 当有对象进入石头位置时触发（可选的碰撞效果）
        private void OnObjectEnter(BehaviorComponentContainer other)
        {
            // 播放碰撞音效或粒子效果
            Debug.Log($"{other.name} 撞到了石头");
        }
    }
}