using HappyHotel.Core.Rarity;

namespace HappyHotel.Reward
{
    // 传说稀有度选择盒奖励物品
    // 被获取时弹出UI，展示3个传说稀有度的装备供选择
    public class LegendarySelectionBoxRewardItem : RaritySelectionBoxRewardItem
    {
        protected override Rarity TargetRarity => Rarity.Legendary;
    }
}