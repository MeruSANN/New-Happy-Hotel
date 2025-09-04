using HappyHotel.Core;

namespace HappyHotel.Prop.Settings
{
    public class PermanentArrowSetting : DirectionalSetting
    {
        public PermanentArrowSetting(Direction direction) : base(direction)
        {
        }

        protected override void ConfigurePropInternal(PropBase prop)
        {
            // 调用父类的通用方向设置逻辑
            base.ConfigurePropInternal(prop);

            // PermanentArrowProp的特殊设置逻辑（如果需要的话）
            if (prop is PermanentArrowProp permanentArrowProp) permanentArrowProp.SetForcedDirection(direction);
        }
    }
}