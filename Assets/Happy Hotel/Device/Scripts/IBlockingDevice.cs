namespace HappyHotel.Device
{
    // 标记Device为阻挡性Device（在墙角计算时被视为墙面）
    public interface IBlockingDevice
    {
        // 是否可被破坏
        bool IsDestructible { get; }
    }
}