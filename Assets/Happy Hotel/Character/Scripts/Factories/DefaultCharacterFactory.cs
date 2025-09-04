using UnityEngine;

namespace HappyHotel.Character.Factories
{
    [CharacterRegistration(
        "Default",
        "Templates/Default Character Template")]
    public class DefaultCharacterFactory : CharacterFactoryBase<DefaultCharacter>
    {
    }
}