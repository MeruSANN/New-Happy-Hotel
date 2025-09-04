using HappyHotel.Core.Registry;
using HappyHotel.Equipment.Factories;
using HappyHotel.Equipment.Settings;
using HappyHotel.Equipment.Templates;
using UnityEngine;

// Ensure correct namespace for WeaponTemplate

namespace HappyHotel.Equipment
{
    public class EquipmentResourceManager : ResourceManagerBase<EquipmentBase, EquipmentTypeId, IEquipmentFactory,
        EquipmentTemplate, IEquipmentSetting>
    {
        // 模板资源的基础路径，相对于 "Resources" 文件夹
        private const string BASE_TEMPLATE_PATH = "Templates/Weapon";

        protected override void LoadTypeResources(EquipmentTypeId typeId)
        {
            var descriptor = EquipmentRegistry.Instance.GetDescriptor(typeId);
            if (descriptor == null)
            {
                Debug.LogError($"WeaponResourceManager: 找不到 WeaponTypeId {typeId.Id} 的描述符。");
                return;
            }

            var resourcePath = string.IsNullOrEmpty(descriptor.TemplatePath)
                ? $"{BASE_TEMPLATE_PATH}/{typeId.Id}" // 如果 TemplatePath 未指定，则默认使用 TypeId 作为文件名
                : descriptor.TemplatePath; // TemplatePath 应该是相对于 Resources 文件夹的完整路径，或者不含扩展名的部分路径

            // 移除可能的 .asset 后缀，因为 Resources.Load不需要它
            if (resourcePath.EndsWith(".asset"))
                resourcePath = resourcePath.Substring(0, resourcePath.Length - ".asset".Length);

            var template = Resources.Load<EquipmentTemplate>(resourcePath);

            if (template != null)
                templateCache[typeId] = template;
            // Debug.Log($"WeaponResourceManager: 已加载并缓存模板 for {typeId.Value} from {resourcePath}");
            else
                Debug.LogWarning(
                    $"WeaponResourceManager: 无法从路径 {resourcePath} 加载 WeaponTemplate for TypeId {typeId.Id}。");
        }

        // 可选：如果希望在编辑器中或通过其他方式手动预加载所有模板
        // public override void LoadAllResources()
        // {
        //     base.LoadAllResources();
        //     Debug.Log("WeaponResourceManager: All weapon templates loaded.");
        // }
    }
}