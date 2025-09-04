using System;
using System.Collections.Generic;
using HappyHotel.Character.Factories;
using HappyHotel.Character.Settings;
using HappyHotel.Character.Templates;
using HappyHotel.Core.Registry;

namespace HappyHotel.Character
{
    public class CharacterRegistry : RegistryBase<CharacterBase, CharacterTypeId, ICharacterFactory, CharacterTemplate,
        ICharacterSetting>
    {
        private readonly Dictionary<CharacterTypeId, CharacterDescriptor> descriptors = new();

        protected override Type GetRegistrationAttributeType()
        {
            return typeof(CharacterRegistrationAttribute);
        }

        protected override void OnRegister(RegistrationAttribute attr)
        {
            var type = GetType(attr.TypeId);
            descriptors[type] = new CharacterDescriptor(type, attr.TemplatePath);
        }

        public CharacterDescriptor GetDescriptor(CharacterTypeId type)
        {
            return descriptors.TryGetValue(type, out var descriptor) ? descriptor : null;
        }

        #region Singleton

        private static CharacterRegistry instance;

        public static CharacterRegistry Instance
        {
            get
            {
                if (instance == null) instance = new CharacterRegistry();
                return instance;
            }
        }

        #endregion
    }
}