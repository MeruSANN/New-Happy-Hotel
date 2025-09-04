using HappyHotel.Core;

namespace HappyHotel.Prop.Settings
{
    public class DirectionChangerSetting : DirectionalSetting
    {
        public DirectionChangerSetting(Direction direction) : base(direction)
        {
        }

        protected override void ConfigurePropInternal(PropBase prop)
        {
            // 调用父类的通用方向设置逻辑
            base.ConfigurePropInternal(prop);

            // DirectionChangerProp的特殊设置逻辑（如果需要的话）
            if (prop is DirectionChangerProp directionProp) directionProp.SetForcedDirection(direction);
        }
    }
}