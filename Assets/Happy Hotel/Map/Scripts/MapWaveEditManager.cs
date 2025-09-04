using System;
using System.Collections.Generic;
using HappyHotel.Core.Singleton;
using HappyHotel.Enemy;
using HappyHotel.Map.Data;
using UnityEngine;
using UnityEngine.SceneManagement;
using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.Grid.Components;
using HappyHotel.Core.Registry;

namespace HappyHotel.Map
{
    // 地图编辑场景用：当前编辑波次管理器
    [ManagedSingleton(SceneLoadMode.Exclude, "ShopScene", "MainMenu", "GameScene")] 
    public class MapWaveEditManager : SingletonBase<MapWaveEditManager>
    {
        private List<WaveConfig> waves = new List<WaveConfig>();
        private int currentWaveIndex;

        public int TotalWaves { get; private set; }
        public int CurrentWaveIndex => currentWaveIndex;

        // 事件：用于通知UI实时同步
        public System.Action onWavesInitialized;
        public Action<int> onTotalWavesChanged;
        public Action<int> onCurrentWaveChanged;
        public Action<int> onCurrentWaveGapChanged;

        protected override void OnSingletonAwake()
        {
            base.OnSingletonAwake();
            // 仅在MapEditScene使用
            var active = SceneManager.GetActiveScene().name;
            if (active != "MapEditScene")
            {
                Destroy(gameObject);
                return;
            }
        }

        public void InitializeFromMap(MapData data)
        {
            waves = data != null && data.waves != null ? new List<WaveConfig>(data.waves) : new List<WaveConfig>();
            TotalWaves = data != null ? Mathf.Max(data.totalWaves, waves.Count) : 0;
            EnsureWaveListSize(TotalWaves);
            currentWaveIndex = Mathf.Clamp(0, 0, Mathf.Max(0, TotalWaves - 1));

            onWavesInitialized?.Invoke();
            onTotalWavesChanged?.Invoke(TotalWaves);
            onCurrentWaveChanged?.Invoke(currentWaveIndex);
            onCurrentWaveGapChanged?.Invoke(GetCurrentWaveGap());
        }

        public void SetTotalWaves(int total)
        {
            TotalWaves = Mathf.Max(0, total);
            EnsureWaveListSize(TotalWaves);
            if (currentWaveIndex >= TotalWaves) currentWaveIndex = Mathf.Max(0, TotalWaves - 1);

            onTotalWavesChanged?.Invoke(TotalWaves);
        }

        public void SetCurrentWaveIndex(int index)
        {
            currentWaveIndex = Mathf.Clamp(index, 0, Mathf.Max(0, TotalWaves - 1));

            onCurrentWaveChanged?.Invoke(currentWaveIndex);
            onCurrentWaveGapChanged?.Invoke(GetCurrentWaveGap());
        }

        public int GetCurrentWaveGap()
        {
            if (!IsValidCurrentWave()) return 0;
            return waves[currentWaveIndex].gapFromPreviousTurns;
        }

        public void SetCurrentWaveGap(int gap)
        {
            if (!IsValidCurrentWave()) return;
            waves[currentWaveIndex].gapFromPreviousTurns = Mathf.Max(0, gap);

            onCurrentWaveGapChanged?.Invoke(GetCurrentWaveGap());
        }

        public void ApplySceneEnemiesToCurrentWave()
        {
            if (!IsValidCurrentWave()) return;
            if (EnemyManager.Instance == null) return;

            var list = new List<WaveEnemy>();
            var enemies = EnemyManager.Instance.GetAllObjects();
            foreach (var enemy in enemies)
            {
                if (enemy == null) continue;
                var container = enemy.GetComponent<BehaviorComponentContainer>();
                var grid = container ? container.GetBehaviorComponent<GridObjectComponent>() : null;
                var type = enemy.TypeId;
                if (grid != null && type != null)
                {
                    list.Add(new WaveEnemy(type.Id, grid.GetGridPosition()));
                }
            }

            waves[currentWaveIndex].enemies = list;
        }

        public void OverwriteSceneEnemiesFromCurrentWave()
        {
            if (!IsValidCurrentWave()) return;
            if (EnemyController.Instance == null) return;

            EnemyController.Instance.ClearAllEnemies();

            var current = waves[currentWaveIndex];
            if (current.enemies == null) return;

            foreach (var we in current.enemies)
            {
                if (string.IsNullOrEmpty(we.enemyTypeId)) continue;
                var typeId = TypeId.Create<EnemyTypeId>(we.enemyTypeId);
                EnemyController.Instance.CreateEnemy(typeId, we.position);
            }
        }

        public List<WaveConfig> GetWavesCopy()
        {
            return new List<WaveConfig>(waves ?? new List<WaveConfig>());
        }

        private bool IsValidCurrentWave()
        {
            return currentWaveIndex >= 0 && currentWaveIndex < waves.Count;
        }

        private void EnsureWaveListSize(int size)
        {
            if (waves == null) waves = new List<WaveConfig>();
            while (waves.Count < size) waves.Add(new WaveConfig());
            while (waves.Count > size) waves.RemoveAt(waves.Count - 1);
        }
    }
}


