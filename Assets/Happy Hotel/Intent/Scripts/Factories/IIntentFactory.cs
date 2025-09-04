using HappyHotel.Core.Registry;
using HappyHotel.Intent.Settings;
using HappyHotel.Intent.Templates;

namespace HappyHotel.Intent.Factories
{
	// 意图工厂接口
	public interface IIntentFactory : IFactory<IntentBase, IntentTemplate, IIntentSetting>
	{
	}
}


