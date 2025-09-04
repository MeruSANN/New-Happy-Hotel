using System.Collections.Generic;
using System.Reflection;
using HappyHotel.Core.EntityComponent;
using HappyHotel.Core.ValueProcessing.Processors;
using UnityEngine;

namespace HappyHotel.Core.ValueProcessing.Components
{
    // EntityComponent版本的ProcessableValue收集器
    public class ProcessableValueCollectorComponent : EntityComponentBase
    {
        private readonly Dictionary<string, ProcessableValue> processableValues = new();

        public override void OnAttach(EntityComponentContainer host)
        {
            base.OnAttach(host);
            CollectProcessableValues();
        }

        // 收集宿主对象中的所有ProcessableValue
        private void CollectProcessableValues()
        {
            if (host == null) return;

            processableValues.Clear();

            var hostType = host.GetType();
            // 只收集私有字段
            var fields = hostType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var field in fields)
                if (typeof(ProcessableValue).IsAssignableFrom(field.FieldType))
                {
                    var processableValue = field.GetValue(host) as ProcessableValue;
                    if (processableValue != null)
                    {
                        processableValues[field.Name] = processableValue;
                        Debug.Log($"收集到ProcessableValue: {field.Name} 在 {host.Name}");
                    }
                }
        }

        // 获取所有ProcessableValue
        public IReadOnlyDictionary<string, ProcessableValue> GetProcessableValues()
        {
            return processableValues;
        }

        // 为所有ProcessableValue注册处理器
        public void RegisterProcessorToAll(IValueProcessor processor)
        {
            foreach (var pv in processableValues.Values) pv.RegisterProcessor(processor);
        }

        // 从所有ProcessableValue注销处理器
        public void UnregisterProcessorFromAll(IValueProcessor processor)
        {
            foreach (var pv in processableValues.Values) pv.UnregisterProcessor(processor);
        }

        // 为所有ProcessableValue注册可叠加处理器
        public void RegisterStackableProcessorToAll<T>(int amount, object provider) where T : IStackableProcessor, new()
        {
            foreach (var pv in processableValues.Values) pv.RegisterStackableProcessor<T>(amount, provider);
        }

        // 从所有ProcessableValue注销可叠加处理器的特定提供者
        public void UnregisterStackableProcessorFromAll<T>(object provider) where T : IStackableProcessor
        {
            foreach (var pv in processableValues.Values) pv.UnregisterStackableProcessor<T>(provider);
        }

        // 手动刷新收集
        public void RefreshCollection()
        {
            CollectProcessableValues();
        }
    }
}