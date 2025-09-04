namespace HappyHotel.Core.ValueProcessing.Modifiers
{
	// 可叠加的数值修饰器（按来源叠加/移除）
	public interface IStackableStatModifier : IStatModifier
	{
		void AddStack(int amount, object provider);
		bool RemoveStack(object provider);
		int GetStackCount();
		bool HasStacks();
		int GetTotalEffectValue();
		bool HasStackFromProvider(object provider);
	}
}
