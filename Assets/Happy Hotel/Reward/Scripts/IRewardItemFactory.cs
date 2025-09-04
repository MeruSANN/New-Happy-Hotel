using HappyHotel.Core.Registry;
using HappyHotel.Reward.Settings;
using HappyHotel.Reward.Templates;

namespace HappyHotel.Reward.Factories
{
    // 奖励物品工厂接口
    public interface IRewardItemFactory : IFactory<RewardItemBase, RewardItemTemplate, IRewardItemSetting>
    {
    }
}