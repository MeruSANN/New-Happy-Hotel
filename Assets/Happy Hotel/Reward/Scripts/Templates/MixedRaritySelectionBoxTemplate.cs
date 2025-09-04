using UnityEngine;

namespace HappyHotel.Reward.Templates
{
    [CreateAssetMenu(fileName = "Mixed Rarity Selection Box Template",
        menuName = "Happy Hotel/Reward/Mixed Rarity Selection Box Template")]
    public class MixedRaritySelectionBoxTemplate : RewardItemTemplate
    {
        [Header("抽选设置")] [Tooltip("每次抽选的道具数量")]
        public int selectionCount = 3;
    }
}