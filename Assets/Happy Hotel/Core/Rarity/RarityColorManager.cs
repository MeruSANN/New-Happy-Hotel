using HappyHotel.Core.Singleton;
using UnityEngine;

namespace HappyHotel.Core.Rarity
{
    // 稀有度管理器
    [ManagedSingleton(true)]
    public class RarityColorManager : SingletonBase<RarityColorManager>
    {
        [Header("稀有度配置")] [SerializeField] private RarityConfig rarityConfig;

        protected override void OnSingletonAwake()
        {
            base.OnSingletonAwake();

            // 如果没有设置稀有度配置，尝试加载默认配置
            if (rarityConfig == null)
            {
                rarityConfig = Resources.Load<RarityConfig>("RarityConfig");
                if (rarityConfig == null) Debug.LogWarning("未找到默认稀有度配置，将使用硬编码的默认颜色");
            }
        }

        // 获取指定稀有度的颜色
        public static Color GetRarityColor(Rarity rarity)
        {
            if (Instance.rarityConfig != null) return Instance.rarityConfig.GetRarityColor(rarity);
            // 没有配置时默认白色
            return Color.white;
        }

        // 设置稀有度配置
        public void SetRarityConfig(RarityConfig config)
        {
            rarityConfig = config;
        }

        // 获取稀有度配置
        public RarityConfig GetRarityConfig()
        {
            return rarityConfig;
        }
    }
}