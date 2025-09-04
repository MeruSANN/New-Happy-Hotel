namespace HappyHotel.Core.Rarity
{
    // 稀有度提供者接口
    public interface IRarityProvider
    {
        // 获取稀有度
        Rarity Rarity { get; }
    }
}