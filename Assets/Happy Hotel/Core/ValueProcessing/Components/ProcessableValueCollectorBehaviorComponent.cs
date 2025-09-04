using System.Collections.Generic;
using System.Reflection;
using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.ValueProcessing.Processors;
using UnityEngine;

namespace HappyHotel.Core.ValueProcessing.Components
{
    // BehaviorComponent版本的ProcessableValue收集器
    public class ProcessableValueCollectorBehaviorComponent : BehaviorComponentBase
    {
        private readonly Dictionary<string, ProcessableValue> processableValues = new();

        public override void OnAttach(BehaviorComponentContainer host)
        {
            base.OnAttach(host);

            CollectProcessableValues();
        }

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

        public IReadOnlyDictionary<string, ProcessableValue> GetProcessableValues()
        {
            return processableValues;
        }

        public void RegisterProcessorToAll(IValueProcessor processor)
        {
            foreach (var pv in processableValues.Values) pv.RegisterProcessor(processor);
        }

        public void UnregisterProcessorFromAll(IValueProcessor processor)
        {
            foreach (var pv in processableValues.Values) pv.UnregisterProcessor(processor);
        }

        public void RegisterStackableProcessorToAll<T>(int amount, object provider) where T : IStackableProcessor, new()
        {
            foreach (var pv in processableValues.Values) pv.RegisterStackableProcessor<T>(amount, provider);
        }

        public void UnregisterStackableProcessorFromAll<T>(object provider) where T : IStackableProcessor
        {
            foreach (var pv in processableValues.Values) pv.UnregisterStackableProcessor<T>(provider);
        }

        public void RefreshCollection()
        {
            CollectProcessableValues();
        }
    }
}