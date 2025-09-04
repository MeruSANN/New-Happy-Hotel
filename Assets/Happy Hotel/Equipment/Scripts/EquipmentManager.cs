using HappyHotel.Core.Registry;
using HappyHotel.Core.Singleton;
using HappyHotel.Equipment.Factories;
using HappyHotel.Equipment.Settings;
using HappyHotel.Equipment.Templates;
using UnityEngine;

// For SingletonBase or ManagedSingletonAttribute

// Ensure correct namespace for WeaponTemplate

namespace HappyHotel.Equipment
{
    // 使用 ManagedSingletonAttribute 使其成为一个自动初始化的单例
    [ManagedSingleton(true)]
    public class EquipmentManager : ManagerBase<EquipmentManager, EquipmentBase, EquipmentTypeId, IEquipmentFactory,
        EquipmentResourceManager, EquipmentTemplate, IEquipmentSetting>
    {
        protected override
            RegistryBase<EquipmentBase, EquipmentTypeId, IEquipmentFactory, EquipmentTemplate, IEquipmentSetting>
            GetRegistry()
        {
            return EquipmentRegistry.Instance;
        }

        // 提供一个方便的方法来获取 ResourceManager (如果需要从外部访问)
        public EquipmentResourceManager GetResourceManager()
        {
            if (!isInitialized) Debug.LogWarning("WeaponManager尚未初始化，ResourceManager可能不可用。");
            return resourceManager; // resourceManager 在 ManagerBase 中定义和初始化
        }

        protected override void Initialize()
        {
            base.Initialize();

            isInitialized = true;
        }
    }
}