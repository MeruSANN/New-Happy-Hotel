namespace HappyHotel.Action.Settings
{
    // 双倍护甲行动设置类
    public class DoubleArmorActionSetting : IActionSetting
    {
        public void ConfigureAction(ActionBase action)
        {
            // 双倍护甲行动不需要额外配置，所有逻辑都在Action内部处理
        }
    }
}