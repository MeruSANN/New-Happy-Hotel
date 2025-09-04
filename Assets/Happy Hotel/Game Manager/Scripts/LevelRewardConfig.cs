using System;
using System.Collections.Generic;
using System.Linq;
using HappyHotel.Core.Registry;
using HappyHotel.Reward;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HappyHotel.GameManager
{
    [Serializable]
    public class LevelRewardConfig
    {
        [Header("关卡类型")] [SerializeField] [Tooltip("关卡类型")]
        public string levelType = "Normal";

        [Header("奖励配置")] [SerializeField] [Tooltip("奖励物品的TypeId列表，包括CoinReward等")]
        [ValueDropdown(nameof(GetAvailableRewardItemTypeIds))]
        public List<string> rewardItemTypeIds = new List<string>();

        private IEnumerable<string> GetAvailableRewardItemTypeIds()
        {
            return RegistryTypeIdUtility.GetRegisteredTypeIdsByRegistrationAttribute<RewardItemRegistrationAttribute>();
        }
    }
}