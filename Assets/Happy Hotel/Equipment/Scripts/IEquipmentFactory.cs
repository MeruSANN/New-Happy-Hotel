using HappyHotel.Core.Registry;
using HappyHotel.Equipment.Settings;
using HappyHotel.Equipment.Templates;

namespace HappyHotel.Equipment.Factories
{
    // 武器工厂接口
    public interface IEquipmentFactory : IFactory<EquipmentBase, EquipmentTemplate, IEquipmentSetting>
    {
    }
}