using System;

namespace HappyHotel.Intent.Settings
{
	// 意图运行时配置接口
	public interface IIntentSetting
	{
		void ConfigureIntent(IntentBase intent);
	}
}


