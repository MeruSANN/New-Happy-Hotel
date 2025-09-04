using HappyHotel.Core.Rarity;

namespace HappyHotel.Reward
{
    // 普通稀有度选择盒奖励物品
    // 被获取时弹出UI，展示3个普通稀有度的装备供选择
    public class CommonSelectionBoxRewardItem : RaritySelectionBoxRewardItem
    {
        protected override Rarity TargetRarity => Rarity.Common;
    }
}