using HappyHotel.Core.Registry;
using HappyHotel.Core.Singleton;
using HappyHotel.Equipment.Templates;
using HappyHotel.Prop.Factories;
using HappyHotel.Prop.Settings;
using UnityEngine;

namespace HappyHotel.Prop
{
    // 道具管理器，负责道具数据加载和创建
    [ManagedSingleton(true)]
    public class PropManager : ManagerBase<PropManager, PropBase, PropTypeId, IPropFactory, PropResourceManager,
        ItemTemplate, IPropSetting>
    {
        protected override RegistryBase<PropBase, PropTypeId, IPropFactory, ItemTemplate, IPropSetting> GetRegistry()
        {
            return PropRegistry.Instance;
        }

        protected override void Initialize()
        {
            base.Initialize();
            isInitialized = true;
        }

        // 提供一个方便的方法来获取 ResourceManager
        public PropResourceManager GetResourceManager()
        {
            if (!isInitialized) Debug.LogWarning("PropManager尚未初始化，ResourceManager可能不可用。");
            return resourceManager;
        }
    }
}