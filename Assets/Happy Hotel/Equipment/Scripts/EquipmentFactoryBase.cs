using System.Reflection;
using HappyHotel.Core.Registry;
using HappyHotel.Equipment.Settings;
using HappyHotel.Equipment.Templates;

namespace HappyHotel.Equipment.Factories
{
    // Weapon工厂基类，提供自动TypeId设置功能
    public abstract class EquipmentFactoryBase<TEquipment> : IEquipmentFactory
        where TEquipment : EquipmentBase, new()
    {
        public EquipmentBase Create(EquipmentTemplate template, IEquipmentSetting setting = null)
        {
            var weapon = new TEquipment();

            // 自动设置TypeId
            AutoSetTypeId(weapon);

            if (template) weapon.SetTemplate(template);
            setting?.ConfigureEquipment(weapon);

            return weapon;
        }

        private void AutoSetTypeId(EquipmentBase equipment)
        {
            var attr = GetType().GetCustomAttribute<EquipmentRegistrationAttribute>();
            if (attr != null)
            {
                var typeId = TypeId.Create<EquipmentTypeId>(attr.TypeId);
                ((ITypeIdSettable<EquipmentTypeId>)equipment).SetTypeId(typeId);
            }
        }
    }
}