namespace HappyHotel.Equipment.Settings
{
    // 武器设置接口，用于在创建武器实例时进行额外配置
    public interface IEquipmentSetting
    {
        void ConfigureEquipment(EquipmentBase equipment);
    }
}