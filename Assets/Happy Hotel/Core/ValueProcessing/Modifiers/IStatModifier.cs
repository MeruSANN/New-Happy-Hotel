namespace HappyHotel.Core.ValueProcessing.Modifiers
{
	// 用于“数值本身”的修饰器
	public interface IStatModifier
	{
		// 优先级，数值越小优先级越高
		int Priority { get; }

		// 对当前值进行修饰并返回
		int Apply(int currentValue);
	}
}