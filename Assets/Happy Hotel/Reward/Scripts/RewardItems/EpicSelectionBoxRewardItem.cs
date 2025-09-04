using HappyHotel.Core.Rarity;

namespace HappyHotel.Reward
{
    // 史诗稀有度选择盒奖励物品
    // 被获取时弹出UI，展示3个史诗稀有度的装备供选择
    public class EpicSelectionBoxRewardItem : RaritySelectionBoxRewardItem
    {
        protected override Rarity TargetRarity => Rarity.Epic;
    }
}