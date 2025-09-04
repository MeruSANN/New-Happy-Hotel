using HappyHotel.Core.Rarity;
using UnityEngine;

namespace HappyHotel.Reward.Templates
{
    // 稀有度选择盒模板
    [CreateAssetMenu(fileName = "RaritySelectionBoxTemplate",
        menuName = "Happy Hotel/Reward/Rarity Selection Box Template")]
    public class RaritySelectionBoxTemplate : RewardItemTemplate
    {
        [Header("稀有度选择盒设置")] public Rarity targetRarity = Rarity.Common;

        public int selectionCount = 3;
    }
}