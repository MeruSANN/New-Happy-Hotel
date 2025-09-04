using System;
using System.Collections.Generic;
using System.Linq;
using HappyHotel.Card.Factories;
using HappyHotel.Card.Setting;
using HappyHotel.Core.Registry;
using HappyHotel.Equipment.Templates;

namespace HappyHotel.Card
{
    public class CardRegistry : RegistryBase<CardBase, CardTypeId, ICardFactory, CardTemplate, ICardSetting>
    {
        private readonly Dictionary<CardTypeId, CardDescriptor> descriptors = new();

        protected override Type GetRegistrationAttributeType()
        {
            return typeof(CardRegistrationAttribute);
        }

        protected override void OnRegister(RegistrationAttribute attr)
        {
            var type = GetType(attr.TypeId);
            descriptors[type] = new CardDescriptor(type, attr.TemplatePath);
        }

        public List<CardDescriptor> GetAllDescriptors()
        {
            return descriptors.Values.ToList();
        }

        public CardDescriptor GetDescriptor(CardTypeId id)
        {
            return descriptors.TryGetValue(id, out var descriptor) ? descriptor : null;
        }

        #region Singleton

        private static CardRegistry instance;

        public static CardRegistry Instance
        {
            get
            {
                if (instance == null) instance = new CardRegistry();
                return instance;
            }
        }

        #endregion
    }
}