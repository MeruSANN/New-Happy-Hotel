using HappyHotel.Core.Registry;
using HappyHotel.Enemy.Factories;
using HappyHotel.Enemy.Settings;
using HappyHotel.Enemy.Templates;
using UnityEngine;

namespace HappyHotel.Enemy
{
    public class
        EnemyResourcesManager : ResourceManagerBase<EnemyBase, EnemyTypeId, IEnemyFactory, EnemyTemplate, IEnemySetting>
    {
        protected override void LoadTypeResources(EnemyTypeId type)
        {
            var descriptor = (registry as EnemyRegistry)!.GetDescriptor(type);

            var template = Resources.Load<EnemyTemplate>(descriptor.TemplatePath);
            if (template)
                templateCache[descriptor.Type] = template;
            else
                Debug.LogWarning($"无法加载敌人模板: {descriptor.TemplatePath}");
        }
    }
}