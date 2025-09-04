using HappyHotel.Buff.Factories;
using HappyHotel.Buff.Templates;
using HappyHotel.Core.Registry;
using UnityEngine;

namespace HappyHotel.Buff
{
    // Buff资源管理器
    public class
        BuffResourceManager : ResourceManagerBase<BuffBase, BuffTypeId, IBuffFactory, BuffTemplate, IBuffSetting>
    {
        protected override void LoadTypeResources(BuffTypeId type)
        {
            var descriptor = ((BuffRegistry)registry).GetDescriptor(type);
            if (descriptor != null && !string.IsNullOrEmpty(descriptor.TemplatePath))
            {
                var template = Resources.Load<BuffTemplate>(descriptor.TemplatePath);
                if (template != null)
                {
                    templateCache[type] = template;
                    Debug.Log($"加载Buff模板: {type} -> {descriptor.TemplatePath}");
                }
                else
                {
                    Debug.LogWarning($"未找到Buff模板: {descriptor.TemplatePath}");
                }
            }
        }
    }
}