using System;
using HappyHotel.Core.EntityComponent;
using HappyHotel.Core.ValueProcessing.Components;
using UnityEngine;

namespace HappyHotel.Action.Components.Parts
{
    // 跟踪护甲值变化的组件，用于监控当前护甲值
    public class ArmorValueTrackerComponent : EntityComponentBase, IEventListener
    {
        private ArmorValueComponent monitoredArmorComponent;

        public int CurrentArmorValue { get; private set; }

        public override void OnAttach(EntityComponentContainer host)
        {
            base.OnAttach(host);

            if (host is not ActionBase) Debug.LogError("ArmorValueTrackerComponent 不能绑定在非ActionBase的容器上！");
        }

        public void OnEvent(EntityComponentEvent evt)
        {
            if (evt.EventName == "SetActionQueue")
            {
                StopMonitoring();
                var actionQueue = evt.Data as ActionQueueComponent;
                if (actionQueue != null && actionQueue.GetHost() != null)
                {
                    var hostBehaviorContainer = actionQueue.GetHost();
                    monitoredArmorComponent = hostBehaviorContainer.GetBehaviorComponent<ArmorValueComponent>();
                    StartMonitoring();
                }
            }
        }

        // 护甲值改变事件
        public event Action<int> onArmorValueChanged;

        // 开始监听指定的ArmorValueComponent
        private void StartMonitoring()
        {
            if (monitoredArmorComponent != null && monitoredArmorComponent.ArmorValue != null)
            {
                monitoredArmorComponent.ArmorValue.onValueChanged.AddListener(OnArmorValueChangedInternal);
                CurrentArmorValue = monitoredArmorComponent.CurrentArmor;
                Debug.Log($"ArmorValueTrackerComponent: 开始监听护甲值，当前护甲: {CurrentArmorValue}");
            }
        }

        // 停止监听
        private void StopMonitoring()
        {
            if (monitoredArmorComponent != null && monitoredArmorComponent.ArmorValue != null)
            {
                monitoredArmorComponent.ArmorValue.onValueChanged.RemoveListener(OnArmorValueChangedInternal);
                monitoredArmorComponent = null;
                Debug.Log("ArmorValueTrackerComponent: 停止监听护甲值");
            }
        }

        public override void OnDestroy()
        {
            // 解除事件监听
            StopMonitoring();
            base.OnDestroy();
        }

        // 处理护甲值变化事件
        private void OnArmorValueChangedInternal(int newValue)
        {
            CurrentArmorValue = newValue;
            Debug.Log($"护甲值变化为: {newValue}");

            // 触发护甲值改变事件
            onArmorValueChanged?.Invoke(CurrentArmorValue);
        }
    }
}