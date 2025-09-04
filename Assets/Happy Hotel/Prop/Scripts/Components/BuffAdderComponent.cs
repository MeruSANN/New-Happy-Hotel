using System;
using HappyHotel.Buff;
using HappyHotel.Buff.Components;
using HappyHotel.Core.BehaviorComponent;
using UnityEngine;

namespace HappyHotel.Prop.Components
{
    // Buff添加组件，当道具被触发时直接给触发者添加Buff
    public class BuffAdderComponent : BehaviorComponentBase, IEventListener
    {
        [SerializeField] private IBuffSetting buffSetting; // Buff设置
        [SerializeField] private string buffType; // 要添加的Buff类型

        public string BuffType
        {
            get => buffType;
            set => buffType = value;
        }

        public IBuffSetting BuffSetting
        {
            get => buffSetting;
            set => buffSetting = value;
        }

        // 实现IEventListener接口，监听Trigger事件
        public void OnEvent(BehaviorComponentEvent evt)
        {
            if (evt.EventName == "Trigger" && evt.Data is BehaviorComponentContainer triggerer)
                AddBuffToTriggerer(triggerer);
        }

        // 添加Buff的方法
        private void AddBuffToTriggerer(BehaviorComponentContainer triggerer)
        {
            if (triggerer == null || string.IsNullOrEmpty(buffType))
                return;

            // 获取触发者的BuffContainer组件
            var buffContainer = triggerer.GetBehaviorComponent<BuffContainer>() ??
                                triggerer.AddBehaviorComponent<BuffContainer>();

            if (buffContainer != null)
            {
                // 通过BuffManager创建Buff实例
                var buff = CreateBuffInstance(buffSetting);
                if (buff != null)
                {
                    // 添加Buff
                    buffContainer.AddBuff(buff);
                    Debug.Log($"{triggerer.gameObject.name} 通过 {host?.gameObject.name} 获得了 {buff.GetType().Name} Buff");
                }
                else
                {
                    Debug.LogWarning($"无法创建Buff实例: {buffType}");
                }
            }
            else
            {
                Debug.LogWarning($"{triggerer.gameObject.name} 没有BuffContainer组件，无法添加Buff");
            }
        }

        // 通过BuffManager创建Buff实例
        private BuffBase CreateBuffInstance(IBuffSetting buffSetting)
        {
            if (!BuffManager.Instance.IsInitialized)
            {
                Debug.LogError("BuffManager尚未初始化");
                return null;
            }

            try
            {
                // 使用BuffManager创建Buff实例
                var buff = BuffManager.Instance.Create(buffType, buffSetting);
                if (buff == null) Debug.LogError($"BuffManager.Create returned null for type={buffType}");
                return buff;
            }
            catch (Exception e)
            {
                Debug.LogError($"创建Buff实例失败: {e.Message}");
                return null;
            }
        }

        // 设置Buff类型
        public void SetBuffType(string type)
        {
            buffType = type;
        }

        // 设置Buff设置
        public void SetBuffSetting(IBuffSetting setting)
        {
            buffSetting = setting;
        }

        // 获取Buff类型
        public string GetBuffType()
        {
            return buffType;
        }

        // 获取Buff设置
        public IBuffSetting GetBuffSetting()
        {
            return buffSetting;
        }
    }
}