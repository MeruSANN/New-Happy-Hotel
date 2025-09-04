using System;
using System.Collections.Generic;
using System.Linq;
using HappyHotel.Core.Registry;
using HappyHotel.Equipment.Factories;
using HappyHotel.Equipment.Settings;
using HappyHotel.Equipment.Templates;

// Corrected namespace for WeaponTemplate

namespace HappyHotel.Equipment
{
    public class EquipmentRegistry : RegistryBase<EquipmentBase, EquipmentTypeId, IEquipmentFactory, EquipmentTemplate,
        IEquipmentSetting>
    {
        // 单例实现
        private static EquipmentRegistry instance;
        private readonly Dictionary<EquipmentTypeId, EquipmentDescriptor> descriptors = new();

        public static EquipmentRegistry Instance
        {
            get
            {
                if (instance == null) instance = new EquipmentRegistry();
                // instance.Initialize(); // Initialization should be handled by a manager or a central point
                return instance;
            }
        }

        protected override Type GetRegistrationAttributeType()
        {
            return typeof(EquipmentRegistrationAttribute);
        }

        protected override void OnRegister(RegistrationAttribute attr)
        {
            // Ensure TypeId is registered with the base class first
            var typeId = base.RegisterType(attr.TypeId);

            // Now that we have the strongly-typed WeaponTypeId, use it for the descriptor
            descriptors[typeId] = new EquipmentDescriptor(typeId, attr.TemplatePath);
            // Debug.Log($"WeaponRegistry: Registered descriptor for {typeId.Value} with template path {attr.TemplatePath}");
        }

        public EquipmentDescriptor GetDescriptor(EquipmentTypeId typeId)
        {
            return descriptors.TryGetValue(typeId, out var descriptor) ? descriptor : null;
        }

        public List<EquipmentDescriptor> GetAllDescriptors()
        {
            return descriptors.Values.ToList();
        }

        // It's good practice to call Initialize explicitly, often from a higher-level manager
        // or a game initialization sequence, rather than in the singleton accessor.
        // public override void Initialize()
        // {
        //     base.Initialize();
        //     Debug.Log("WeaponRegistry Initialized and factories scanned.");
        // }
    }
}