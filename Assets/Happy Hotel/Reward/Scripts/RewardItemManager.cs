using HappyHotel.Core.Registry;
using HappyHotel.Core.Singleton;
using HappyHotel.Reward.Factories;
using HappyHotel.Reward.Settings;
using HappyHotel.Reward.Templates;
using UnityEngine;

namespace HappyHotel.Reward
{
    // 奖励物品管理器，负责创建和管理奖励物品
    [ManagedSingleton(true)]
    public class RewardItemManager : ManagerBase<RewardItemManager, RewardItemBase, RewardItemTypeId, IRewardItemFactory
        , RewardItemResourceManager, RewardItemTemplate, IRewardItemSetting>
    {
        protected override
            RegistryBase<RewardItemBase, RewardItemTypeId, IRewardItemFactory, RewardItemTemplate, IRewardItemSetting>
            GetRegistry()
        {
            return RewardItemRegistry.Instance;
        }

        protected override void Initialize()
        {
            base.Initialize();
            isInitialized = true;
        }

        public RewardItemBase CreateRewardItem(string typeIdString, IRewardItemSetting setting = null)
        {
            var typeId = TypeId.Create<RewardItemTypeId>(typeIdString);
            return CreateRewardItem(typeId, setting);
        }

        // 创建奖励物品
        public RewardItemBase CreateRewardItem(RewardItemTypeId itemType, IRewardItemSetting setting = null)
        {
            if (!isInitialized)
            {
                Debug.LogError("RewardItemManager未初始化，无法创建奖励物品");
                return null;
            }

            var rewardItem = Create(itemType, setting);
            if (rewardItem)
            {
                rewardItem.transform.parent = transform;
                Debug.Log($"已创建奖励物品: {rewardItem.ItemName}");
            }

            return rewardItem;
        }

        // 获取模板数据
        public RewardItemTemplate GetTemplate(RewardItemTypeId typeId)
        {
            return resourceManager.GetTemplate(typeId);
        }
    }
}