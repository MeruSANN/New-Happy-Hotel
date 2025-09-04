using System;
using System.Collections.Generic;
using HappyHotel.Core.Registry;
using HappyHotel.Core.Singleton;
using HappyHotel.Enemy;
using HappyHotel.Map.Data;
using UnityEngine;

namespace HappyHotel.GameManager
{
	// 波次刷新管理器
	[ManagedSingleton(SceneLoadMode.Exclude, "ShopScene", "MainMenu", "MapEditScene")]
	public class WaveSpawnManager : SingletonBase<WaveSpawnManager>
	{
		// 当前关卡波次数据
		private List<WaveConfig> waves = new List<WaveConfig>();

		// 下一波索引
		private int nextWaveIndex;

		// 下一波计划触发的回合数（<0 为未计划）
		private int nextWaveScheduledTurn = -1;

		// 防止同一回合内重复触发多次刷新的保护
		private int lastSpawnProcessedTurn = -1;

		// 防止重复触发过关
		private bool completionTriggered;

		public bool AllWavesSpawned => nextWaveIndex >= waves.Count;

		public bool IsAllWavesCleared => AllWavesSpawned && EnemyController.Instance != null &&
		                                  EnemyController.Instance.GetCurrentEnemyCount() == 0;

		public int GetNextWaveScheduledTurn() => nextWaveScheduledTurn;

		public int GetTurnsUntilNextScheduledWave()
		{
			if (AllWavesSpawned) return -1;
			if (nextWaveScheduledTurn < 0) return -1;
			var currentTurn = TurnManager.Instance != null ? TurnManager.Instance.GetCurrentTurn() : 1;
			var delta = nextWaveScheduledTurn - currentTurn;
			return Mathf.Max(-1, delta);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			TurnManager.onPlayerTurnStart -= OnTurnStart;
			GameManager.onAllEnemiesStateChanged -= OnAllEnemiesStateChanged;
		}

		protected override void OnSingletonAwake()
		{
			base.OnSingletonAwake();
			TurnManager.onPlayerTurnStart += OnTurnStart;
			GameManager.onAllEnemiesStateChanged += OnAllEnemiesStateChanged;
		}

		// 初始化（关卡加载成功后调用）
		public void InitializeFromMap(MapData data)
		{
			waves = data?.waves != null ? new List<WaveConfig>(data.waves) : new List<WaveConfig>();
			// 第1波由MapStorageManager在加载地图时生成；此处从第2波开始管理
			nextWaveIndex = waves.Count > 0 ? 1 : 0;
			nextWaveScheduledTurn = -1;
			completionTriggered = false;

			// 预排第二波触发点
			ScheduleNextWaveAfterGap();

			// 初始化后检查一次通关条件
			CheckAndCompleteIfCleared();
		}

		private void OnTurnStart(int turnNumber)
		{
			if (AllWavesSpawned)
			{
				CheckAndCompleteIfCleared();
				return;
			}
			if (nextWaveScheduledTurn >= 0 && turnNumber >= nextWaveScheduledTurn)
			{
				TrySpawnNextWave("TurnReached");
			}

			// 回合开始时检查通关条件
			CheckAndCompleteIfCleared();
		}

		private void OnAllEnemiesStateChanged(bool allDead)
		{
			if (!allDead) return;
			if (AllWavesSpawned)
			{
				CheckAndCompleteIfCleared();
				return;
			}
			// 敌人清空后不立刻刷新，改为在下一次玩家回合开始时刷新
			ScheduleNextWaveOnNextPlayerTurn();
		}

		private void TrySpawnNextWave(string reason)
		{
			if (AllWavesSpawned) return;

			var currentTurn = TurnManager.Instance != null ? TurnManager.Instance.GetCurrentTurn() : -1;

			// 同一回合仅处理一次刷新
			if (currentTurn >= 0 && lastSpawnProcessedTurn == currentTurn)
			{
				return;
			}

			// 防重复：仅当未刷且满足任一触发条件时才刷
			SpawnWave(nextWaveIndex, reason);
			ScheduleNextWaveAfterGap();

			if (currentTurn >= 0) lastSpawnProcessedTurn = currentTurn;
		}

		private void CheckAndCompleteIfCleared()
		{
			if (completionTriggered) return;
			if (!IsAllWavesCleared) return;
			if (LevelCompletionManager.Instance == null) return;

			completionTriggered = true;
			LevelCompletionManager.Instance.CompleteLevel();
		}

		private void SpawnWave(int waveIndex, string reason)
		{
			if (waveIndex < 0 || waveIndex >= waves.Count) return;

			var wave = waves[waveIndex];
			if (wave == null || wave.enemies == null || wave.enemies.Count == 0)
			{
				nextWaveIndex = Mathf.Max(nextWaveIndex, waveIndex + 1);
				return;
			}

			foreach (var we in wave.enemies)
			{
				if (string.IsNullOrEmpty(we.enemyTypeId)) continue;
				var typeId = TypeId.Create<EnemyTypeId>(we.enemyTypeId);
				EnemyController.Instance.CreateEnemy(typeId, we.position);
			}

			nextWaveIndex = waveIndex + 1;
		}

		private void ScheduleNextWaveAfterGap()
		{
			if (AllWavesSpawned)
			{
				nextWaveScheduledTurn = -1;
				return;
			}

			var currentTurn = TurnManager.Instance != null ? TurnManager.Instance.GetCurrentTurn() : 1;

			// 第1波已在关卡开始立即刷新；从第2波起使用gap
			var gap = 0;
			var waveIdx = nextWaveIndex;
			var wave = waveIdx >= 0 && waveIdx < waves.Count ? waves[waveIdx] : null;
			gap = Mathf.Max(0, wave != null ? wave.gapFromPreviousTurns : 0);

			nextWaveScheduledTurn = currentTurn + gap;
		}

		private void ScheduleNextWaveOnNextPlayerTurn()
		{
			if (AllWavesSpawned)
			{
				nextWaveScheduledTurn = -1;
				return;
			}

			var currentTurn = TurnManager.Instance != null ? TurnManager.Instance.GetCurrentTurn() : 1;
			// 将下一波安排到下一次玩家回合开始
			nextWaveScheduledTurn = currentTurn + 1;
		}
	}
}


