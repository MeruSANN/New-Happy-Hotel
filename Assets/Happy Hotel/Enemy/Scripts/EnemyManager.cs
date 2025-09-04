using HappyHotel.Core.Registry;
using HappyHotel.Core.Singleton;
using HappyHotel.Enemy.Factories;
using HappyHotel.Enemy.Settings;
using HappyHotel.Enemy.Templates;
using UnityEngine;

namespace HappyHotel.Enemy
{
    // 敌人管理器，负责敌人数据加载和创建
    [ManagedSingleton(true)]
    public class EnemyManager : ManagerBase<EnemyManager, EnemyBase, EnemyTypeId, IEnemyFactory, EnemyResourcesManager,
        EnemyTemplate, IEnemySetting>
    {
        protected override RegistryBase<EnemyBase, EnemyTypeId, IEnemyFactory, EnemyTemplate, IEnemySetting>
            GetRegistry()
        {
            return EnemyRegistry.Instance;
        }

        protected override void Initialize()
        {
            base.Initialize();
            isInitialized = true;
        }

        // 提供一个方便的方法来获取 ResourceManager
        public EnemyResourcesManager GetResourceManager()
        {
            if (!isInitialized) Debug.LogWarning("EnemyManager尚未初始化，ResourceManager可能不可用。");
            return resourceManager;
        }
    }
}