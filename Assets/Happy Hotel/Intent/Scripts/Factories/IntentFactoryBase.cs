using System.Reflection;
using HappyHotel.Core.Registry;
using HappyHotel.Intent.Settings;
using HappyHotel.Intent.Templates;

namespace HappyHotel.Intent.Factories
{
	// 意图工厂基类，提供TypeId自动设置
	public abstract class IntentFactoryBase<TIntent> : IIntentFactory
		where TIntent : IntentBase, new()
	{
		public IntentBase Create(IntentTemplate template, IIntentSetting setting = null)
		{
			var intent = new TIntent();
			AutoSetTypeId(intent);
			if (template != null) intent.SetTemplate(template);
			setting?.ConfigureIntent(intent);
			return intent;
		}

		private void AutoSetTypeId(IntentBase intent)
		{
			var attr = GetType().GetCustomAttribute<IntentRegistrationAttribute>();
			if (attr != null)
			{
				var typeId = TypeId.Create<IntentTypeId>(attr.TypeId);
				((ITypeIdSettable<IntentTypeId>)intent).SetTypeId(typeId);
			}
		}
	}
}


