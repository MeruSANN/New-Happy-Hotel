using UnityEngine;

namespace HappyHotel.Reward.Templates
{
    // 卡牌混合稀有度选择盒模板
    [CreateAssetMenu(fileName = "CardMixedRaritySelectionBoxTemplate",
        menuName = "Happy Hotel/Reward/Card Mixed Rarity Selection Box Template")]
    public class CardMixedRaritySelectionBoxTemplate : RewardItemTemplate
    {
        [Header("卡牌抽选设置")] [Tooltip("每次抽选的卡牌数量")]
        public int selectionCount = 3;
    }
}