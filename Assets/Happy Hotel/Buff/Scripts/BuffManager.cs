using HappyHotel.Buff.Factories;
using HappyHotel.Buff.Templates;
using HappyHotel.Core.Registry;
using HappyHotel.Core.Singleton;
using UnityEngine;

namespace HappyHotel.Buff
{
    // Buff管理器
    [ManagedSingleton(true)]
    public class BuffManager : ManagerBase<BuffManager, BuffBase, BuffTypeId, IBuffFactory, BuffResourceManager,
        BuffTemplate, IBuffSetting>
    {
        protected override RegistryBase<BuffBase, BuffTypeId, IBuffFactory, BuffTemplate, IBuffSetting> GetRegistry()
        {
            return BuffRegistry.Instance;
        }

        protected override void Initialize()
        {
            base.Initialize();
            isInitialized = true;
        }

        // 提供一个方便的方法来获取 ResourceManager
        public BuffResourceManager GetResourceManager()
        {
            if (!isInitialized) Debug.LogWarning("BuffManager尚未初始化，ResourceManager可能不可用。");
            return resourceManager;
        }
    }
}