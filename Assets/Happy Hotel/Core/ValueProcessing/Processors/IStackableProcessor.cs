namespace HappyHotel.Core.ValueProcessing.Processors
{
    // 可叠加处理器接口，用于解决重复Processor实例的问题
    public interface IStackableProcessor : IValueProcessor
    {
        // 添加效果叠加（provider: 效果提供者/归属来源）
        void AddStack(int amount, object provider);

        // 移除效果叠加（provider: 效果提供者/归属来源）
        bool RemoveStack(object provider);

        // 获取当前叠加数量
        int GetStackCount();

        // 是否仍有效果
        bool HasStacks();

        // 获取总效果值
        int GetTotalEffectValue();

        // 检查是否包含来自指定提供者的叠加
        bool HasStackFromProvider(object provider);
    }
}