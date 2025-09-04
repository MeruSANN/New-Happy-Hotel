using System;
using System.Linq;
using HappyHotel.Core.Singleton;
using HappyHotel.Enemy;
using HappyHotel.Inventory;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace HappyHotel.GameManager
{
    [ManagedSingleton(SceneLoadMode.Exclude, "MapEditScene", "ShopScene", "MainMenu")]
    public class GameManager : SingletonBase<GameManager>
    {
        // 游戏状态枚举
        public enum GameState
        {
            Idle, // 待机状态
            Playing, // 播放状态
            Reward // 奖励状态（关卡完成后显示奖励面板）
        }

        private readonly KeyCode playToggleKey = KeyCode.Space;

        // 当前游戏状态
        private GameState currentGameState = GameState.Idle;

        // 用于跟踪敌人死亡的标记
        private bool hasCheckedEnemyDeathThisFrame;

        private void Update()
        {
            // 在奖励状态下阻止所有玩家操作
            if (currentGameState == GameState.Reward) return;

            // 检测开始/暂停按键
            if (Input.GetKeyDown(playToggleKey))
                if (currentGameState == GameState.Idle)
                    SetGameState(GameState.Playing);

            // 在游戏播放状态下检查敌人死亡情况
            if (currentGameState == GameState.Playing && !hasCheckedEnemyDeathThisFrame)
            {
                CheckAllEnemiesDead();
                hasCheckedEnemyDeathThisFrame = true;
            }
        }

        private void LateUpdate()
        {
            // 重置检查标记，确保每帧只检查一次
            hasCheckedEnemyDeathThisFrame = false;
        }

        private void OnEnable()
        {
            // 监听游戏状态变化
            onGameStateChanged += HandleGameStateChanged;
        }

        private void OnDisable()
        {
            onGameStateChanged -= HandleGameStateChanged;
        }

        // 游戏状态改变事件
        public static event Action<GameState> onGameStateChanged;

        // 敌人状态变化事件
        public static event Action<bool> onAllEnemiesStateChanged; // true表示所有敌人都死亡，false表示还有敌人存活

        // 获取当前游戏状态
        public GameState GetGameState()
        {
            return currentGameState;
        }

        // 设置游戏状态
        public void SetGameState(GameState newState)
        {
            if (currentGameState != newState)
            {
                var previousState = currentGameState;
                currentGameState = newState;

                onGameStateChanged?.Invoke(currentGameState);

                if (currentGameState == GameState.Playing)
                    Debug.Log("游戏开始播放");
                else
                    Debug.Log("游戏暂停");
            }
        }

        // 当角色碰到墙体时调用
        public async void OnHitWall()
        {
            // 先立即进入静止，防止时钟系统继续移动
            SetGameState(GameState.Idle);
            Debug.Log("碰到墙体，游戏暂停");

            // 然后推进回合（异步等待敌人回合执行完成）
            if (TurnManager.Instance != null) await TurnManager.Instance.AdvanceTurnAsync();
        }

        // 处理游戏状态变化
        private void HandleGameStateChanged(GameState newState)
        {
            if (newState == GameState.Idle)
                // 游戏进入静止状态
                // 检查敌人状态来决定出口装置的状态，而不是直接重置为不可通过
                CheckAllEnemiesDead();
            else if (newState == GameState.Playing)
                // 游戏进入播放状态，处理临时区卡牌
                ProcessTemporaryCards();
        }

        // 处理临时区卡牌（新增）
        private void ProcessTemporaryCards()
        {
            if (CardInventory.Instance != null)
            {
                // 获取临时区的所有卡牌
                var temporaryCards = CardInventory.Instance.GetTemporaryCards();

                foreach (var card in temporaryCards.ToList())
                {
                    // 根据卡牌类型决定最终去向
                    var targetZone = card.IsConsumable
                        ? CardInventory.CardZone.Consumed
                        : CardInventory.CardZone.Discard;

                    // 先从手牌区移除卡牌（如果存在）
                    if (CardInventory.Instance.HasCardOfType(card.TypeId, CardInventory.CardZone.Hand))
                    {
                        CardInventory.Instance.RemoveFromTemporaryZone(card);
                        CardInventory.Instance.MoveCardToZone(card, targetZone);
                    }
                    else
                    {
                        // 将卡牌从临时区移除并移动到目标区域
                        CardInventory.Instance.MoveCardToZone(card, targetZone);
                    }
                }

                Debug.Log("已处理临时区卡牌");
            }
        }

        // 检查所有敌人是否都死亡（公共方法，供外部调用）
        public void CheckAllEnemiesDead()
        {
            if (!EnemyManager.Instance) return;

            // 获取所有敌人
            var allEnemies = EnemyManager.Instance.GetAllObjects();

            // 如果没有敌人，触发所有敌人死亡事件
            if (allEnemies.Count == 0)
            {
                onAllEnemiesStateChanged?.Invoke(true);
                return;
            }

            // 检查是否所有敌人都已死亡（被销毁）
            var allEnemiesDead = true;
            foreach (var enemy in allEnemies)
                if (enemy != null && enemy.gameObject != null)
                {
                    allEnemiesDead = false;
                    break;
                }

            // 触发敌人状态变化事件
            onAllEnemiesStateChanged?.Invoke(allEnemiesDead);
        }
    }
}