using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.Grid.Components;
using HappyHotel.Core.ValueProcessing.Components;
using UnityEngine;

namespace HappyHotel.Device
{
    public class SpikeDevice : DeviceBase
    {
        private int damage = 1;

        protected override void Awake()
        {
            base.Awake();

            // 监听网格对象进入事件
            var gridComponent = GetBehaviorComponent<GridObjectComponent>();
            gridComponent.onObjectEnter.AddListener(OnObjectEnter);
        }

        private void OnObjectEnter(BehaviorComponentContainer other)
        {
            // 地刺被触发时造成伤害
            var healthComponent = other.GetBehaviorComponent<HitPointValueComponent>();
            if (healthComponent != null)
            {
                healthComponent.TakeDamage(damage, this);
                Debug.Log($"地刺对 {other.name} 造成了 {damage} 点伤害");
            }
        }

        public void SetDamage(int newDamage)
        {
            damage = newDamage;
        }
    }
}