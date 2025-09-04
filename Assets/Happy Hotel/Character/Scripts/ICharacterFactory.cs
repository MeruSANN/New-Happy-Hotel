using HappyHotel.Character.Settings;
using HappyHotel.Character.Templates;
using HappyHotel.Core.Registry;

namespace HappyHotel.Character.Factories
{
    public interface ICharacterFactory : IFactory<CharacterBase, CharacterTemplate, ICharacterSetting>
    {
    }
}