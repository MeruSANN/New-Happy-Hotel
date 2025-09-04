namespace HappyHotel.Equipment.Settings
{
    // 装备设置基类，包含通用的装备配置信息
    public abstract class EquipmentSettingBase : IEquipmentSetting
    {
        public virtual void ConfigureEquipment(EquipmentBase equipment)
        {
            // 调用子类的具体配置
            ConfigureEquipmentInternal(equipment);
        }

        // 子类重写此方法来实现具体的装备配置
        protected abstract void ConfigureEquipmentInternal(EquipmentBase equipment);
    }
}