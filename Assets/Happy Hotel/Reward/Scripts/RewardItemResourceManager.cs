using HappyHotel.Core.Registry;
using HappyHotel.Reward.Factories;
using HappyHotel.Reward.Settings;
using HappyHotel.Reward.Templates;
using UnityEngine;

namespace HappyHotel.Reward
{
    // 奖励物品资源管理器
    public class RewardItemResourceManager : ResourceManagerBase<RewardItemBase, RewardItemTypeId, IRewardItemFactory,
        RewardItemTemplate, IRewardItemSetting>
    {
        protected override void LoadTypeResources(RewardItemTypeId type)
        {
            var descriptor = (registry as RewardItemRegistry)!.GetDescriptor(type);

            var template = Resources.Load<RewardItemTemplate>(descriptor.TemplatePath);
            if (template != null)
                templateCache[descriptor.TypeId] = template;
            else
                Debug.LogWarning($"无法加载奖励道具模板: {descriptor.TemplatePath}");
        }
    }
}