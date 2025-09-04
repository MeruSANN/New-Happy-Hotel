using System;
using System.Collections.Generic;
using System.Linq;
using HappyHotel.Core.Registry;
using HappyHotel.Equipment.Templates;
using HappyHotel.Prop.Factories;
using HappyHotel.Prop.Settings;

namespace HappyHotel.Prop
{
    public class PropRegistry : RegistryBase<PropBase, PropTypeId, IPropFactory, ItemTemplate, IPropSetting>
    {
        private readonly Dictionary<PropTypeId, PropDescriptor> descriptors = new();

        protected override void OnRegister(RegistrationAttribute attr)
        {
            var type = GetType(attr.TypeId);
            descriptors[type] = new PropDescriptor(type, attr.TemplatePath);
        }

        protected override Type GetRegistrationAttributeType()
        {
            return typeof(PropRegistrationAttribute);
        }

        public List<PropDescriptor> GetAllDescriptors()
        {
            return descriptors.Values.ToList();
        }

        public PropDescriptor GetDescriptor(PropTypeId id)
        {
            return descriptors[id];
        }

        #region Singleton

        private static PropRegistry instance;

        public static PropRegistry Instance
        {
            get
            {
                if (instance == null) instance = new PropRegistry();
                return instance;
            }
        }

        #endregion
    }
}