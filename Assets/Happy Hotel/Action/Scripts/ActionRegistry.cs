using System;
using System.Collections.Generic;
using System.Linq;
using HappyHotel.Action.Factories;
using HappyHotel.Action.Settings;
using HappyHotel.Action.Templates;
using HappyHotel.Core.Registry;

namespace HappyHotel.Action
{
    public class ActionRegistry : RegistryBase<ActionBase, ActionTypeId, IActionFactory, ActionTemplate, IActionSetting>
    {
        private readonly Dictionary<ActionTypeId, ActionDescriptor> descriptors = new();

        protected override Type GetRegistrationAttributeType()
        {
            return typeof(ActionRegistrationAttribute);
        }

        protected override void OnRegister(RegistrationAttribute attr)
        {
            var type = GetType(attr.TypeId);
            descriptors[type] = new ActionDescriptor(type, attr.TemplatePath);
        }

        public List<ActionDescriptor> GetAllDescriptors()
        {
            return descriptors.Values.ToList();
        }

        public ActionDescriptor GetDescriptor(ActionTypeId id)
        {
            return descriptors[id];
        }

        #region Singleton

        private static ActionRegistry instance;

        public static ActionRegistry Instance
        {
            get
            {
                if (instance == null) instance = new ActionRegistry();
                return instance;
            }
        }

        #endregion
    }
}