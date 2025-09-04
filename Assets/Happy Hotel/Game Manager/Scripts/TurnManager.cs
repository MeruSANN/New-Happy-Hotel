using System;
using HappyHotel.Core.Singleton;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace HappyHotel.GameManager
{
    // 回合管理器，负责追踪和控制游戏回合的开始与结束
    [ManagedSingleton(SceneLoadMode.Exclude, "ShopScene", "MainMenu", "MapEditScene")]
    public class TurnManager : SingletonBase<TurnManager>
    {
        // 当前回合数（0表示游戏尚未开始）
        private int currentTurn;

        // 阶段
        public enum TurnPhase { Player, Enemy }
        private TurnPhase currentPhase = TurnPhase.Player;

        // 新增分阶段事件
        public static event Action<int> onPlayerTurnStart;
        public static event Action<int> onPlayerTurnEnd;
        public static event Action<int> onEnemyTurnStart;
        public static event Action<int> onEnemyTurnEnd;

        // 开始游戏的第一个回合。此方法只应在游戏初始化时调用一次。
        public void StartFirstTurn()
        {
            if (currentTurn == 0)
            {
                currentTurn = 1;
                currentPhase = TurnPhase.Player;
                Debug.Log("游戏开始，进入第一回合。");
                onPlayerTurnStart?.Invoke(currentTurn);
            }
            else
            {
                Debug.LogWarning("StartFirstTurn() 被多次调用，但游戏已经开始。");
            }
        }

        // 结束当前回合，并开始下一个回合（异步版本）。
        public async UniTask AdvanceTurnAsync()
        {
            if (currentTurn <= 0)
            {
                Debug.LogError("游戏尚未开始，无法推进回合。请先调用 StartFirstTurn()。");
                return;
            }

            // 玩家回合结束 -> 敌人回合开始
            onPlayerTurnEnd?.Invoke(currentTurn);
            currentPhase = TurnPhase.Enemy;
            Debug.Log($"进入敌人回合（第 {currentTurn} 回合）。");
            onEnemyTurnStart?.Invoke(currentTurn);

            // 等待所有敌人意图执行完成
            if (EnemyTurnCoordinator.Instance != null)
            {
                await EnemyTurnCoordinator.Instance.ExecuteEnemyTurnAsync();
            }

            // 敌人回合结束
            onEnemyTurnEnd?.Invoke(currentTurn);
            Debug.Log($"第 {currentTurn} 回合结束。");

            // 进入下一回合玩家回合
            currentTurn++;
            currentPhase = TurnPhase.Player;
            Debug.Log($"进入第 {currentTurn} 回合。");
            onPlayerTurnStart?.Invoke(currentTurn);
        }

        // 获取当前回合数。
        public int GetCurrentTurn()
        {
            return currentTurn;
        }

        // 获取当前阶段
        public TurnPhase GetCurrentPhase()
        {
            return currentPhase;
        }

        // 重置回合计数器，用于新游戏或新关卡。
        public void ResetTurn()
        {
            currentTurn = 0;
            currentPhase = TurnPhase.Player;
            Debug.Log("回合数已重置为0。");
        }
    }
}