using System;
using System.Collections.Generic;
using System.Linq;
using HappyHotel.Core.Registry;
using HappyHotel.Reward.Factories;
using HappyHotel.Reward.Settings;
using HappyHotel.Reward.Templates;

namespace HappyHotel.Reward
{
    // 奖励物品注册表
    public class RewardItemRegistry : RegistryBase<RewardItemBase, RewardItemTypeId, IRewardItemFactory,
        RewardItemTemplate, IRewardItemSetting>
    {
        private static RewardItemRegistry instance;

        private readonly Dictionary<RewardItemTypeId, RewardItemDescriptor> descriptors = new();

        public static RewardItemRegistry Instance
        {
            get
            {
                if (instance == null) instance = new RewardItemRegistry();
                return instance;
            }
        }

        protected override void OnRegister(RegistrationAttribute attr)
        {
            var type = GetType(attr.TypeId);
            descriptors[type] = new RewardItemDescriptor(type, attr.TemplatePath);
        }

        public List<RewardItemDescriptor> GetAllDescriptors()
        {
            return descriptors.Values.ToList();
        }

        protected override Type GetRegistrationAttributeType()
        {
            return typeof(RewardItemRegistrationAttribute);
        }

        public RewardItemDescriptor GetDescriptor(RewardItemTypeId id)
        {
            return descriptors[id];
        }
    }
}