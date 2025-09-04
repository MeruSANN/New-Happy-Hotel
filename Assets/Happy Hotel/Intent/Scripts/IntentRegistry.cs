using System;
using System.Collections.Generic;
using System.Linq;
using HappyHotel.Core.Registry;
using HappyHotel.Intent.Factories;
using HappyHotel.Intent.Settings;
using HappyHotel.Intent.Templates;

namespace HappyHotel.Intent
{
	public class IntentRegistry : RegistryBase<IntentBase, IntentTypeId, IIntentFactory, IntentTemplate, IIntentSetting>
	{
		private readonly Dictionary<IntentTypeId, IntentDescriptor> descriptors = new();

		protected override Type GetRegistrationAttributeType()
		{
			return typeof(IntentRegistrationAttribute);
		}

		protected override void OnRegister(RegistrationAttribute attr)
		{
			var type = GetType(attr.TypeId);
			descriptors[type] = new IntentDescriptor(type, attr.TemplatePath);
		}

		public List<IntentDescriptor> GetAllDescriptors()
		{
			return descriptors.Values.ToList();
		}

		public IntentDescriptor GetDescriptor(IntentTypeId id)
		{
			return descriptors[id];
		}

		private static IntentRegistry instance;

		public static IntentRegistry Instance
		{
			get
			{
				if (instance == null) instance = new IntentRegistry();
				return instance;
			}
		}
	}
}


