namespace HappyHotel.Core.ValueProcessing.Modifiers
{
	// 可选：让修饰器感知其所属的管理器，便于在内部事件中反向操作管理器
	public interface IModifierManagerAware
	{
		void BindManager(StatModifierManager manager);
		void UnbindManager(StatModifierManager manager);
	}
}

