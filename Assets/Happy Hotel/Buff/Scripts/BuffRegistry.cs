using System;
using System.Collections.Generic;
using System.Linq;
using HappyHotel.Buff.Factories;
using HappyHotel.Buff.Templates;
using HappyHotel.Core.Registry;

namespace HappyHotel.Buff
{
    public class BuffRegistry : RegistryBase<BuffBase, BuffTypeId, IBuffFactory, BuffTemplate, IBuffSetting>
    {
        private readonly Dictionary<BuffTypeId, BuffDescriptor> descriptors = new();

        protected override Type GetRegistrationAttributeType()
        {
            return typeof(BuffRegistrationAttribute);
        }

        protected override void OnRegister(RegistrationAttribute attr)
        {
            var type = GetType(attr.TypeId);
            descriptors[type] = new BuffDescriptor(type, attr.TemplatePath);
        }

        public List<BuffDescriptor> GetAllDescriptors()
        {
            return descriptors.Values.ToList();
        }

        public BuffDescriptor GetDescriptor(BuffTypeId id)
        {
            return descriptors.TryGetValue(id, out var descriptor) ? descriptor : null;
        }

        #region Singleton

        private static BuffRegistry instance;

        public static BuffRegistry Instance
        {
            get
            {
                if (instance == null) instance = new BuffRegistry();
                return instance;
            }
        }

        #endregion
    }
}