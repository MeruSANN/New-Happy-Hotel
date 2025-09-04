using HappyHotel.Core.Registry;
using HappyHotel.Equipment.Templates;
using HappyHotel.Prop.Settings;

namespace HappyHotel.Prop.Factories
{
    public interface IPropFactory : IFactory<PropBase, ItemTemplate, IPropSetting>
    {
    }
}