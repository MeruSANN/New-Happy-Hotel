using System;
using System.Collections.Generic;
using HappyHotel.Core.Singleton;
using UnityEngine;

namespace HappyHotel.GameManager
{
    // 费用管理器，负责处理玩家的费用（能量）
    [ManagedSingleton(SceneLoadMode.Exclude, "ShopScene", "MainMenu", "MapEditScene")]
    [SingletonInitializationDependency(typeof(TurnManager))]
    [SingletonInitializationDependency(typeof(ConfigProvider))]
    public class CostManager : SingletonBase<CostManager>
    {
        // 当前费用
        public int CurrentCost { get; private set; }

        // 费用上限
        public int MaxCost { get; private set; }

        // 当关费用上限加成（按提供者聚合）
        private readonly Dictionary<object, int> levelMaxCostBonusByProvider = new();

        protected override void Awake()
        {
            base.Awake();
            // 订阅玩家回合开始事件
            TurnManager.onPlayerTurnStart += RestoreCostToMax;

            // 订阅关卡生命周期事件以便清理当关效果
            LevelCompletionManager.onLevelCompleted += OnLevelEnded;
            LevelCompletionManager.onEnteringShop += OnEnteringShop;
            LevelCompletionManager.onEnteringNextLevel += OnEnteringNextLevel;

            // 从ConfigProvider初始化费用上限
            var provider = ConfigProvider.Instance;
            var config = provider ? provider.GetGameConfig() : null;
            if (config != null)
            {
                InitializeCost(config.InitialMaxCost);
            }
            else
            {
                InitializeCost(3);
                Debug.LogWarning("[CostManager] 无法从ConfigProvider获取GameConfig，使用默认最大费用 3");
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            // 取消订阅以防内存泄漏
            TurnManager.onPlayerTurnStart -= RestoreCostToMax;

            LevelCompletionManager.onLevelCompleted -= OnLevelEnded;
            LevelCompletionManager.onEnteringShop -= OnEnteringShop;
            LevelCompletionManager.onEnteringNextLevel -= OnEnteringNextLevel;
        }

        // 当费用变化时触发的事件
        public static event Action<int, int> onCostChanged;

        // 初始化费用
        private void InitializeCost(int maxCost)
        {
            MaxCost = maxCost;
            CurrentCost = MaxCost;
            onCostChanged?.Invoke(CurrentCost, MaxCost);
            Debug.Log($"费用系统已初始化，最大费用为 {MaxCost}。");
        }

        // 检查费用是否足够
        public bool HasEnoughCost(int amount)
        {
            return CurrentCost >= amount;
        }

        // 使用费用
        public bool UseCost(int amount)
        {
            if (!HasEnoughCost(amount))
            {
                Debug.LogWarning($"费用不足！当前费用：{CurrentCost}, 需要：{amount}");
                return false;
            }

            CurrentCost -= amount;
            onCostChanged?.Invoke(CurrentCost, MaxCost);
            Debug.Log($"使用了 {amount} 费用，剩余 {CurrentCost}。");
            return true;
        }

        // 增加费用
        public void AddCost(int amount)
        {
            CurrentCost = Mathf.Min(CurrentCost + amount, MaxCost);
            onCostChanged?.Invoke(CurrentCost, MaxCost);
        }

        // 静默设置费用与上限（用于恢复）；可选择是否触发事件
        public void SetCost(int current, int max, bool raiseEvent)
        {
            MaxCost = Mathf.Max(0, max);
            CurrentCost = Mathf.Clamp(current, 0, MaxCost);
            if (raiseEvent) onCostChanged?.Invoke(CurrentCost, MaxCost);
        }

        // 每回合开始时将费用恢复至最大值
        private void RestoreCostToMax(int turnNumber)
        {
            Debug.Log($"回合 {turnNumber} 开始，费用已恢复。");
            CurrentCost = MaxCost;
            onCostChanged?.Invoke(CurrentCost, MaxCost);
        }

        // 对外接口：添加当关费用上限加成（按提供者记录）
        public void AddLevelMaxCostBonus(int amount, object provider)
        {
            if (amount <= 0) return;
            var key = provider ?? this;
            levelMaxCostBonusByProvider.TryGetValue(key, out var prev);
            levelMaxCostBonusByProvider[key] = prev + amount;
            ApplyMaxCostDelta(amount);
        }

        // 对外接口：移除指定提供者的当关费用上限加成
        public void RemoveLevelMaxCostBonus(object provider)
        {
            var key = provider ?? this;
            if (!levelMaxCostBonusByProvider.TryGetValue(key, out var value) || value == 0) return;
            levelMaxCostBonusByProvider.Remove(key);
            ApplyMaxCostDelta(-value);
        }

        // 清空当关所有费用上限加成
        private void ClearAllLevelMaxCostBonuses()
        {
            var total = 0;
            foreach (var kv in levelMaxCostBonusByProvider) total += kv.Value;
            if (total != 0) ApplyMaxCostDelta(-total);
            levelMaxCostBonusByProvider.Clear();
        }

        // 内部：应用上限变化并钳制当前值
        private void ApplyMaxCostDelta(int delta)
        {
            var newMax = Mathf.Max(0, MaxCost + delta);
            var newCur = Mathf.Clamp(CurrentCost, 0, newMax);
            MaxCost = newMax;
            CurrentCost = newCur;
            onCostChanged?.Invoke(CurrentCost, MaxCost);
        }

        // 关卡事件回调
        private void OnLevelEnded(string levelName)
        {
            ClearAllLevelMaxCostBonuses();
        }

        private void OnEnteringShop(string levelName)
        {
            ClearAllLevelMaxCostBonuses();
        }

        private void OnEnteringNextLevel(string currentLevel, int branchIndex)
        {
            ClearAllLevelMaxCostBonuses();
        }
    }
}