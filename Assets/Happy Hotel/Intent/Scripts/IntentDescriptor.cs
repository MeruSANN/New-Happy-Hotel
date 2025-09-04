namespace HappyHotel.Intent
{
	// 意图描述符：记录类型与模板资源路径
	public class IntentDescriptor
	{
		public IntentDescriptor(IntentTypeId type, string templatePath)
		{
			Type = type;
			TemplatePath = templatePath;
		}

		public IntentTypeId Type { get; }
		public string TemplatePath { get; }
	}
}


