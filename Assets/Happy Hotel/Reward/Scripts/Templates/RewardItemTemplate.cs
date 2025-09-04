using HappyHotel.Core.Rarity;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HappyHotel.Reward.Templates
{
    // 奖励物品模板基类
    public class RewardItemTemplate : ScriptableObject
    {
        [Header("基本信息")] public string itemName;

        public string description;

        [PreviewField] public Sprite icon;

        public Rarity rarity = Rarity.Common;
    }
}