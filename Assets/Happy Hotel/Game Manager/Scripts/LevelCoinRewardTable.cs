using System;
using UnityEngine;

namespace HappyHotel.GameManager
{
    [Serializable]
    public class LevelCoinRewardTable
    {
        [SerializeField] [Tooltip("普通关卡的基础金币奖励")]
        public int normal;

        [SerializeField] [Tooltip("Boss关卡的基础金币奖励")]
        public int boss;

        [SerializeField] [Tooltip("精英关卡的基础金币奖励")]
        public int elite;

        [SerializeField] [Tooltip("教程关卡的基础金币奖励")]
        public int tutorial;

        // 根据关卡类型获取金币奖励
        public int GetCoinReward(LevelType levelType)
        {
            switch (levelType)
            {
                case LevelType.Normal:
                    return normal;
                case LevelType.Boss:
                    return boss;
                case LevelType.Elite:
                    return elite;
                case LevelType.Tutorial:
                    return tutorial;
                default:
                    return 0;
            }
        }

        // 设置指定关卡类型的金币奖励
        public void SetCoinReward(LevelType levelType, int reward)
        {
            switch (levelType)
            {
                case LevelType.Normal:
                    normal = reward;
                    break;
                case LevelType.Boss:
                    boss = reward;
                    break;
                case LevelType.Elite:
                    elite = reward;
                    break;
                case LevelType.Tutorial:
                    tutorial = reward;
                    break;
            }
        }
    }
}