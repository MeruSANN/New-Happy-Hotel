using System;
using HappyHotel.Buff;
using HappyHotel.Buff.Components;
using HappyHotel.Core.EntityComponent;
using UnityEngine;
using IEventListener = HappyHotel.Core.EntityComponent.IEventListener;

namespace HappyHotel.Action.Components.Parts
{
    [ExecutionPriority(100)]
    public class SelfBuffApplierEntityComponent : EntityComponentBase, IEventListener
    {
        private IBuffSetting buffSetting;
        private string buffTypeString;

        // 实现IEventListener接口，处理事件
        public void OnEvent(EntityComponentEvent evt)
        {
            if (evt.EventName == "Execute" && evt.Data is ActionQueueComponent actionQueue) ApplyBuff(actionQueue);
        }

        // 设置要应用的Buff类型和配置
        public void SetBuffToApply(string buffType, IBuffSetting setting = null)
        {
            buffTypeString = buffType;
            buffSetting = setting;
        }

        // 应用Buff到行动发起者
        private void ApplyBuff(ActionQueueComponent actionQueue)
        {
            if (string.IsNullOrEmpty(buffTypeString))
            {
                Debug.LogError("没有设置要应用的Buff类型");
                return;
            }

            // 获取行动发起者
            if (actionQueue == null || actionQueue.GetHost() == null)
            {
                Debug.LogError("无法获取行动发起者");
                return;
            }

            var hostBehaviorContainer = actionQueue.GetHost();

            // 获取目标的BuffContainer组件，没有则添加
            var buffContainer = hostBehaviorContainer.GetBehaviorComponent<BuffContainer>() ??
                                hostBehaviorContainer.AddBehaviorComponent<BuffContainer>();

            // 创建新的Buff实例
            var buffToApply = CreateBuffInstance();
            if (buffToApply == null)
            {
                Debug.LogError($"无法创建Buff实例: {buffTypeString}");
                return;
            }

            // 添加Buff到目标
            buffContainer.AddBuff(buffToApply);

            Debug.Log($"向 {hostBehaviorContainer.name} 应用了 {buffToApply.GetType().Name}");
        }

        // 通过BuffManager创建Buff实例
        private BuffBase CreateBuffInstance()
        {
            if (!BuffManager.Instance.IsInitialized)
            {
                Debug.LogError("BuffManager尚未初始化");
                return null;
            }

            try
            {
                // 使用BuffManager创建Buff实例
                var buff = BuffManager.Instance.Create(buffTypeString, buffSetting);
                return buff;
            }
            catch (Exception e)
            {
                Debug.LogError($"创建Buff实例失败: {e.Message}");
                return null;
            }
        }

        // 获取当前设置的Buff类型字符串
        public string GetBuffTypeString()
        {
            return buffTypeString;
        }

        // 获取当前的Buff设置
        public IBuffSetting GetBuffSetting()
        {
            return buffSetting;
        }

        // 清理资源
        public override void OnDestroy()
        {
            base.OnDestroy();
            buffTypeString = null;
            buffSetting = null;
        }
    }
}