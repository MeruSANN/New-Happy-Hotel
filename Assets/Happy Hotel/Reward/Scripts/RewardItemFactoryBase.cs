using HappyHotel.Reward.Settings;
using HappyHotel.Reward.Templates;
using UnityEngine;

namespace HappyHotel.Reward.Factories
{
    // 奖励物品工厂基类
    public abstract class RewardItemFactoryBase<T> : IRewardItemFactory
        where T : RewardItemBase
    {
        public virtual RewardItemBase Create(RewardItemTemplate template, IRewardItemSetting settings)
        {
            // 创建GameObject
            var gameObject = new GameObject(typeof(T).Name);

            // 添加组件
            var rewardItem = gameObject.AddComponent<T>();

            // 设置模板和设置
            if (rewardItem != null)
            {
                rewardItem.SetTemplate(template);
                OnRewardItemCreated(rewardItem, settings);
            }

            return rewardItem;
        }

        // 子类可以重写此方法来添加额外的初始化逻辑
        protected virtual void OnRewardItemCreated(RewardItemBase rewardItem, IRewardItemSetting settings)
        {
            // 应用设置
            if (settings != null) ApplySettings(rewardItem, settings);
        }

        // 应用设置到奖励物品
        private void ApplySettings(RewardItemBase rewardItem, IRewardItemSetting settings)
        {
            if (settings is CoinRewardSetting coinSetting) coinSetting.ConfigureRewardItem(rewardItem);
            // 可以添加其他设置类型的处理
        }
    }
}