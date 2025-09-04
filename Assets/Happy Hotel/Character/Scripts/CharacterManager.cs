using HappyHotel.Character.Factories;
using HappyHotel.Character.Settings;
using HappyHotel.Character.Templates;
using HappyHotel.Core.Registry;
using HappyHotel.Core.Singleton;
using UnityEngine;

namespace HappyHotel.Character
{
    // 角色管理器，负责角色数据加载和创建
    [ManagedSingleton(true)]
    public class CharacterManager : ManagerBase<CharacterManager, CharacterBase, CharacterTypeId, ICharacterFactory,
        CharacterResourcesManager, CharacterTemplate, ICharacterSetting>
    {
        protected override
            RegistryBase<CharacterBase, CharacterTypeId, ICharacterFactory, CharacterTemplate, ICharacterSetting>
            GetRegistry()
        {
            return CharacterRegistry.Instance;
        }

        protected override void Initialize()
        {
            base.Initialize();
            isInitialized = true;
        }

        // 提供一个方便的方法来获取 ResourceManager
        public CharacterResourcesManager GetResourceManager()
        {
            if (!isInitialized) Debug.LogWarning("CharacterManager尚未初始化，ResourceManager可能不可用。");
            return resourceManager;
        }
    }
}