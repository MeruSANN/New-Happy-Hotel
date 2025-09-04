using System.Collections.Generic;
using System.Linq;
using HappyHotel.Core.Registry;
using HappyHotel.Reward;
using HappyHotel.Core.Rarity;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HappyHotel.GameManager
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Happy Hotel/Game Manager/Game Config")]
    public class GameConfig : SerializedScriptableObject
    {
        [Header("道具生成配置")] [SerializeField] [Tooltip("从背包刷新道具的数量上限")]
        private int maxPropSpawnCount = 10;

        [Header("回合抽牌配置")] [SerializeField] [Tooltip("每回合开始时抽取的卡牌数量")]
        private int cardsToDrawPerTurn = 5;

        [Header("费用配置")] [SerializeField] [Tooltip("玩家初始最大费用")]
        private int initialMaxCost = 3;

        [Header("金币奖励倍数配置")] [SerializeField] [Tooltip("第一回合通关的金币奖励倍数")]
        private float firstTurnCoinMultiplier = 1.5f;

        [SerializeField] [Tooltip("曲线下降速度（值越大下降越快）")]
        private float curveDecayRate = 0.3f;

        [SerializeField] [Tooltip("预定回合数（此回合及之后倍数固定为1.0）")]
        private int targetTurnForBaseReward = 10;

        [SerializeField] [Tooltip("最终金币奖励的随机范围最小值")]
        private float finalRewardRandomMin = 0.9f;

        [SerializeField] [Tooltip("最终金币奖励的随机范围最大值")]
        private float finalRewardRandomMax = 1.1f;

        [Header("UI界面设置")] [SerializeField] [Tooltip("过关后是否显示奖励界面")]
        private bool showRewardPanelAfterCompletion = true;

        [Header("关卡奖励配置")] [SerializeField] [Tooltip("关卡类型与奖励的对应关系")]
        private LevelRewardConfig[] levelRewardConfigs = new LevelRewardConfig[0];

        [Header("关卡金币奖励表格")]
        [SerializeField]
        [Tooltip("关卡难度和类型对应的基础金币奖励表格")]
        [TableList(ShowIndexLabels = true, AlwaysExpanded = true)]
        private LevelCoinRewardTable[] levelCoinRewardTable = new LevelCoinRewardTable[0];

        [Header("商店道具价格配置")] [SerializeField] [Tooltip("稀有度与基础价格的映射")]
        private Dictionary<Rarity, int> rarityPriceDict = new();

        [SerializeField] [Tooltip("商店道具价格浮动百分比（如0.1表示±10%）")]
        private float priceRandomRangePercent = 0.1f;

        // 获取道具刷新数量上限
        public int MaxPropSpawnCount => maxPropSpawnCount;

        // 获取每回合抽牌数
        public int CardsToDrawPerTurn => cardsToDrawPerTurn;

        // 获取初始最大费用
        public int InitialMaxCost => initialMaxCost;

        // 获取第一回合金币倍数
        public float FirstTurnCoinMultiplier => firstTurnCoinMultiplier;

        // 获取曲线下降速度
        public float CurveDecayRate => curveDecayRate;

        // 获取预定回合数
        public int TargetTurnForBaseReward => targetTurnForBaseReward;

        // 获取最终随机范围
        public float FinalRewardRandomMin => finalRewardRandomMin;
        public float FinalRewardRandomMax => finalRewardRandomMax;

        // 获取是否显示奖励界面
        public bool ShowRewardPanelAfterCompletion => showRewardPanelAfterCompletion;

        // 获取关卡奖励配置
        public LevelRewardConfig[] LevelRewardConfigs => levelRewardConfigs;

        // 提供一个 RewardItem TypeId 的下拉（用于 Editor 端辅助）
        private IEnumerable<string> GetAvailableRewardItemTypeIds()
        {
            return RegistryTypeIdUtility.GetRegisteredTypeIdsByRegistrationAttribute<RewardItemRegistrationAttribute>();
        }

        // 获取关卡金币奖励表格
        public LevelCoinRewardTable[] LevelCoinRewardTable => levelCoinRewardTable;

        // 计算指定回合的金币奖励倍数
        public float CalculateCoinMultiplier(int completionTurn)
        {
            if (completionTurn <= 0)
                return 1.0f;

            if (completionTurn >= targetTurnForBaseReward)
                return 1.0f;

            // 使用指数衰减函数：multiplier = 1 + (firstTurnMultiplier - 1) * e^(-decayRate * (turn - 1))
            var baseMultiplier = firstTurnCoinMultiplier - 1.0f;
            var decayFactor = Mathf.Exp(-curveDecayRate * (completionTurn - 1));
            return 1.0f + baseMultiplier * decayFactor;
        }

        // 计算最终金币奖励（应用倍数和随机因子）
        public int CalculateFinalCoinReward(int baseCoinReward, int completionTurn)
        {
            if (baseCoinReward <= 0)
                return 0;

            // 计算倍数后的奖励
            var multiplier = CalculateCoinMultiplier(completionTurn);
            var multipliedReward = baseCoinReward * multiplier;

            // 应用随机因子
            var randomFactor = Random.Range(finalRewardRandomMin, finalRewardRandomMax);
            var finalReward = multipliedReward * randomFactor;

            return Mathf.RoundToInt(finalReward);
        }

        // 获取未来回合的倍数预览（用于Inspector显示）
        public string GetMultiplierPreview()
        {
            var preview = "金币奖励倍数预览：\n";
            for (var turn = 1; turn <= targetTurnForBaseReward; turn++)
            {
                var multiplier = CalculateCoinMultiplier(turn);
                preview += $"第{turn}回合: {multiplier:F2}x\n";
            }

            preview += $"第{targetTurnForBaseReward}回合及之后: 1.00x";
            return preview;
        }

        // 检查配置是否有效
        public bool IsValid()
        {
            // 检查道具刷新数量上限是否为负数
            if (maxPropSpawnCount < 0)
                return false;

            if (cardsToDrawPerTurn < 0)
                return false;

            // 检查金币奖励配置
            if (firstTurnCoinMultiplier < 1.0f)
                return false;

            if (curveDecayRate < 0.0f)
                return false;

            if (targetTurnForBaseReward < 1)
                return false;

            if (finalRewardRandomMin < 0.0f || finalRewardRandomMax < finalRewardRandomMin)
                return false;

            return true;
        }

        // 根据关卡类型获取奖励配置
        public LevelRewardConfig GetLevelRewardConfig(LevelType levelType)
        {
            foreach (var config in levelRewardConfigs)
                if (config.levelType == levelType.ToString())
                    return config;

            return null;
        }

        // 根据关卡类型和难度获取基础金币奖励
        public int GetBaseCoinReward(LevelType levelType, int difficulty)
        {
            if (difficulty >= 0 && difficulty <= levelCoinRewardTable.Length)
                return levelCoinRewardTable[difficulty - 1].GetCoinReward(levelType);
            return 0; // 默认无奖励
        }

        // 获取指定稀有度的基础价格
        public int GetBasePriceByRarity(Rarity rarity)
        {
            if (rarityPriceDict == null) return 0;

            if (rarityPriceDict.TryGetValue(rarity, out var price)) return price;

            return 0;
        }

        // 获取带浮动的价格
        public int GetRandomizedPrice(Rarity rarity)
        {
            var basePrice = GetBasePriceByRarity(rarity);
            var min = 1f - priceRandomRangePercent;
            var max = 1f + priceRandomRangePercent;
            var randomFactor = Random.Range(min, max);
            return Mathf.RoundToInt(basePrice * randomFactor);
        }

        // 获取价格浮动百分比
        public float GetPriceRandomRangePercent()
        {
            return priceRandomRangePercent;
        }

        // 获取所有稀有度价格配置的副本
        public Dictionary<Rarity, int> GetRarityPriceConfig()
        {
            var config = new Dictionary<Rarity, int>();
            if (rarityPriceDict != null)
                foreach (var kvp in rarityPriceDict)
                    config[kvp.Key] = kvp.Value;

            return config;
        }
    }
}