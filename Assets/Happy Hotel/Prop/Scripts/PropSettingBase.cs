namespace HappyHotel.Prop.Settings
{
    // Prop设置基类，包含通用的Prop配置信息
    public abstract class PropSettingBase : IPropSetting
    {
        public virtual void ConfigureProp(PropBase prop)
        {
            // 调用子类的具体配置
            ConfigurePropInternal(prop);
        }

        // 子类重写此方法来实现具体的Prop配置
        protected virtual void ConfigurePropInternal(PropBase prop)
        {
        }
    }
}