using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace HappyHotel.Core.Rarity
{
    // 稀有度配置ScriptableObject
    [CreateAssetMenu(fileName = "RarityConfig", menuName = "Happy Hotel/Rarity Config")]
    public class RarityConfig : SerializedScriptableObject
    {
        [Header("稀有度颜色配置（字典）")]
        [DictionaryDrawerSettings(KeyLabel = "稀有度", ValueLabel = "颜色")]
        [SerializeField]
        [OdinSerialize]
        private Dictionary<Rarity, Color> rarityColors = new()
        {
            { Rarity.Common, Color.white },
            { Rarity.Rare, Color.blue },
            { Rarity.Epic, Color.magenta },
            { Rarity.Legendary, Color.yellow }
        };

        [Header("稀有度概率配置（字典，0~1，总和建议为1）")]
        [DictionaryDrawerSettings(KeyLabel = "稀有度", ValueLabel = "概率(0~1)")]
        [SerializeField]
        [OdinSerialize]
        private Dictionary<Rarity, float> rarityProbabilities = new()
        {
            { Rarity.Common, 0.7f },
            { Rarity.Rare, 0.2f },
            { Rarity.Epic, 0.08f },
            { Rarity.Legendary, 0.02f }
        };

        // 获取指定稀有度的颜色
        public Color GetRarityColor(Rarity rarity)
        {
            if (rarityColors != null && rarityColors.TryGetValue(rarity, out var color))
                return color;
            return Color.white;
        }

        // 获取所有稀有度颜色
        public Dictionary<Rarity, Color> GetAllRarityColors()
        {
            return rarityColors;
        }

        // 获取指定稀有度的概率
        public float GetRarityProbability(Rarity rarity)
        {
            if (rarityProbabilities != null && rarityProbabilities.TryGetValue(rarity, out var prob))
                return prob;
            return 0f;
        }

        // 获取所有稀有度及其概率
        public Dictionary<Rarity, float> GetAllRarityProbabilities()
        {
            return rarityProbabilities;
        }
    }
}