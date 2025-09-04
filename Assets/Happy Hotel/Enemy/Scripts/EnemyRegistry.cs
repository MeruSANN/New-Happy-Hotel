using System;
using System.Collections.Generic;
using System.Linq;
using HappyHotel.Core.Registry;
using HappyHotel.Enemy.Factories;
using HappyHotel.Enemy.Settings;
using HappyHotel.Enemy.Templates;

namespace HappyHotel.Enemy
{
    public class EnemyRegistry : RegistryBase<EnemyBase, EnemyTypeId, IEnemyFactory, EnemyTemplate, IEnemySetting>
    {
        private readonly Dictionary<EnemyTypeId, EnemyDescriptor> descriptors = new();

        protected override Type GetRegistrationAttributeType()
        {
            return typeof(EnemyRegistrationAttribute);
        }

        protected override void OnRegister(RegistrationAttribute attr)
        {
            var type = GetType(attr.TypeId);
            descriptors[type] = new EnemyDescriptor(type, attr.TemplatePath);
        }

        public EnemyDescriptor GetDescriptor(EnemyTypeId type)
        {
            return descriptors.TryGetValue(type, out var descriptor) ? descriptor : null;
        }

        public List<EnemyDescriptor> GetAllDescriptors()
        {
            return descriptors.Values.ToList();
        }

        #region Singleton

        private static EnemyRegistry instance;

        public static EnemyRegistry Instance
        {
            get
            {
                if (instance == null) instance = new EnemyRegistry();
                return instance;
            }
        }

        #endregion
    }
}