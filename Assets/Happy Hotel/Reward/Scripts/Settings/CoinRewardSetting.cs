using System;
using UnityEngine;

namespace HappyHotel.Reward.Settings
{
    // 金币奖励设置类，用于配置获得的金币数量
    [Serializable]
    public class CoinRewardSetting : IRewardItemSetting
    {
        private int coinAmount;

        public CoinRewardSetting(int coinAmount)
        {
            this.coinAmount = Mathf.Max(0, coinAmount);
        }

        public void ConfigureRewardItem(RewardItemBase rewardItem)
        {
            if (rewardItem is CoinRewardItem coinRewardItem) coinRewardItem.SetCoinAmount(coinAmount);
        }
    }
}