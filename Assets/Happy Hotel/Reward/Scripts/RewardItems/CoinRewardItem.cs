using HappyHotel.Shop;
using UnityEngine;

namespace HappyHotel.Reward
{
    // 金币奖励物品实现
    public class CoinRewardItem : RewardItemBase
    {
        // 奖励的金币数量

        public int CoinAmount { get; private set; } = 10;

        protected override bool OnExecute()
        {
            base.OnExecute();

            // 金币奖励的执行逻辑
            Debug.Log($"获取了金币奖励，将获得 {CoinAmount} 金币");

            // 直接增加金币
            AddCoinsToPlayer();

            return true; // 金币奖励立即完成
        }

        // 增加金币的方法
        public void AddCoinsToPlayer()
        {
            if (ShopMoneyManager.Instance != null)
            {
                var beforeCoins = ShopMoneyManager.Instance.CurrentMoney;
                ShopMoneyManager.Instance.AddMoney(CoinAmount);
                var afterCoins = ShopMoneyManager.Instance.CurrentMoney;

                Debug.Log($"金币奖励生效！获得 {CoinAmount} 金币 ({beforeCoins} -> {afterCoins})");
            }
            else
            {
                Debug.LogError("ShopMoneyManager实例未找到，无法增加金币");
            }
        }

        // 设置金币数量
        public void SetCoinAmount(int amount)
        {
            CoinAmount = Mathf.Max(0, amount);
        }

        protected override string FormatDescriptionInternal(string formattedDescription)
        {
            // 为金币奖励添加特定的占位符替换
            return formattedDescription
                .Replace("{coinAmount}", CoinAmount.ToString());
        }
    }
}