using HappyHotel.Character.Factories;
using HappyHotel.Character.Settings;
using HappyHotel.Character.Templates;
using HappyHotel.Core.Registry;
using UnityEngine;

namespace HappyHotel.Character
{
    public class CharacterResourcesManager : ResourceManagerBase<CharacterBase, CharacterTypeId, ICharacterFactory,
        CharacterTemplate, ICharacterSetting>
    {
        protected override void LoadTypeResources(CharacterTypeId type)
        {
            var descriptor = (registry as CharacterRegistry)!.GetDescriptor(type);

            var template = Resources.Load<CharacterTemplate>(descriptor.TemplatePath);
            if (template != null)
                templateCache[descriptor.Type] = template;
            else
                Debug.LogWarning($"无法加载角色模板: {descriptor.TemplatePath}");
        }
    }
}