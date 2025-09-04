using HappyHotel.Core.Rarity;
using UnityEngine;

namespace HappyHotel.Reward.Templates
{
    // 卡牌稀有度选择盒模板
    [CreateAssetMenu(fileName = "CardRaritySelectionBoxTemplate",
        menuName = "Happy Hotel/Reward/Card Rarity Selection Box Template")]
    public class CardRaritySelectionBoxTemplate : RewardItemTemplate
    {
        [Header("卡牌稀有度选择盒设置")] public Rarity targetRarity = Rarity.Common;

        public int selectionCount = 3;
    }
}