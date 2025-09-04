using HappyHotel.Core.Rarity;

namespace HappyHotel.Reward
{
    // 稀有稀有度选择盒奖励物品
    // 被获取时弹出UI，展示3个稀有稀有度的装备供选择
    public class RareSelectionBoxRewardItem : RaritySelectionBoxRewardItem
    {
        protected override Rarity TargetRarity => Rarity.Rare;
    }
}