using HappyHotel.Core.Registry;
using HappyHotel.Reward.Settings;
using HappyHotel.Reward.Templates;
using UnityEngine;

namespace HappyHotel.Reward
{
    // 奖励控制器，用于管理和执行奖励物品
    public class RewardController : MonoBehaviour
    {
        [Header("奖励配置")] [SerializeField] private string coinRewardTypeId = "CoinReward";

        [SerializeField] private int coinAmount = 50;

        [SerializeField] private string commonSelectionBoxTypeId = "CommonSelectionBox";
        [SerializeField] private string rareSelectionBoxTypeId = "RareSelectionBox";
        [SerializeField] private string epicSelectionBoxTypeId = "EpicSelectionBox";
        [SerializeField] private string legendarySelectionBoxTypeId = "LegendarySelectionBox";
        [SerializeField] private string mixedRaritySelectionBoxTypeId = "MixedRaritySelectionBox";

        // 执行金币奖励
        public void ExecuteCoinReward()
        {
            var coinSetting = new CoinRewardSetting(coinAmount);
            var rewardItem = RewardItemManager.Instance.CreateRewardItem(coinRewardTypeId, coinSetting);

            if (rewardItem != null)
            {
                rewardItem.Execute();
                Debug.Log($"执行了金币奖励，获得 {coinAmount} 金币");
            }
            else
            {
                Debug.LogError($"无法创建金币奖励物品: {coinRewardTypeId}");
            }
        }

        // 执行普通稀有度选择盒奖励
        public void ExecuteCommonSelectionBox()
        {
            var rewardItem = RewardItemManager.Instance.CreateRewardItem(commonSelectionBoxTypeId);

            if (rewardItem != null)
            {
                rewardItem.Execute();
                Debug.Log("执行了普通稀有度选择盒奖励");
            }
            else
            {
                Debug.LogError($"无法创建普通稀有度选择盒奖励物品: {commonSelectionBoxTypeId}");
            }
        }

        // 执行稀有稀有度选择盒奖励
        public void ExecuteRareSelectionBox()
        {
            var rewardItem = RewardItemManager.Instance.CreateRewardItem(rareSelectionBoxTypeId);

            if (rewardItem != null)
            {
                rewardItem.Execute();
                Debug.Log("执行了稀有稀有度选择盒奖励");
            }
            else
            {
                Debug.LogError($"无法创建稀有稀有度选择盒奖励物品: {rareSelectionBoxTypeId}");
            }
        }

        // 执行史诗稀有度选择盒奖励
        public void ExecuteEpicSelectionBox()
        {
            var rewardItem = RewardItemManager.Instance.CreateRewardItem(epicSelectionBoxTypeId);

            if (rewardItem != null)
            {
                rewardItem.Execute();
                Debug.Log("执行了史诗稀有度选择盒奖励");
            }
            else
            {
                Debug.LogError($"无法创建史诗稀有度选择盒奖励物品: {epicSelectionBoxTypeId}");
            }
        }

        // 执行传说稀有度选择盒奖励
        public void ExecuteLegendarySelectionBox()
        {
            var rewardItem = RewardItemManager.Instance.CreateRewardItem(legendarySelectionBoxTypeId);

            if (rewardItem != null)
            {
                rewardItem.Execute();
                Debug.Log("执行了传说稀有度选择盒奖励");
            }
            else
            {
                Debug.LogError($"无法创建传说稀有度选择盒奖励物品: {legendarySelectionBoxTypeId}");
            }
        }

        // 执行混合稀有度选择盒奖励
        public void ExecuteMixedRaritySelectionBox()
        {
            var rewardItem = RewardItemManager.Instance.CreateRewardItem(mixedRaritySelectionBoxTypeId);

            if (rewardItem != null)
            {
                rewardItem.Execute();
                Debug.Log("执行了混合稀有度选择盒奖励");
            }
            else
            {
                Debug.LogError($"无法创建混合稀有度选择盒奖励物品: {mixedRaritySelectionBoxTypeId}");
            }
        }

        // 通过TypeId执行奖励
        public void ExecuteRewardByTypeId(string typeId, IRewardItemSetting setting = null)
        {
            var rewardItem = RewardItemManager.Instance.CreateRewardItem(typeId, setting);

            if (rewardItem != null)
            {
                rewardItem.Execute();
                Debug.Log($"执行了奖励物品: {typeId}");
            }
            else
            {
                Debug.LogError($"无法创建奖励物品: {typeId}");
            }
        }

        // 获取奖励物品模板
        public RewardItemTemplate GetRewardTemplate(string typeId)
        {
            var typeIdObj = TypeId.Create<RewardItemTypeId>(typeId);
            return RewardItemManager.Instance.GetTemplate(typeIdObj);
        }
    }
}