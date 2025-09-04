using HappyHotel.Map.Data;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HappyHotel.Map
{
    // 一个脚本中实现3个控制器：总波次数、选择波次、当前波次gap输入
    public class MapWaveEditUIController : HappyHotel.Utils.SingletonConnectedUIBase<MapWaveEditManager>
    {
        [Header("UI组件")]
        [SerializeField] private TMP_InputField totalWavesInput;
        [SerializeField] private TMP_Dropdown waveIndexDropdown;
        [SerializeField] private TMP_InputField currentWaveGapInput;

        protected override void OnUIStart()
        {
            InitializeFromCurrentMap();
            BindUIEvents();
            RefreshDropdownOptions();
            RefreshCurrentWaveUI();
            RefreshSceneByCurrentWave();
        }

        private void InitializeFromCurrentMap()
        {
            var mapData = MapStorageManager.Instance != null ? MapStorageManager.Instance.GetCurrentMapData() : null;
            if (MapWaveEditManager.Instance != null) MapWaveEditManager.Instance.InitializeFromMap(mapData);
        }

        private void BindUIEvents()
        {
            if (totalWavesInput != null)
                totalWavesInput.onEndEdit.AddListener(OnTotalWavesChanged);

            if (waveIndexDropdown != null)
                waveIndexDropdown.onValueChanged.AddListener(OnWaveIndexChanged);

            if (currentWaveGapInput != null)
                currentWaveGapInput.onEndEdit.AddListener(OnWaveGapChanged);
        }

        protected override void OnSingletonConnected()
        {
            // 订阅单例事件以实时更新UI
            if (!IsConnectedToSingleton()) return;
            singletonInstance.onWavesInitialized += HandleWavesInitialized;
            singletonInstance.onTotalWavesChanged += HandleTotalWavesChanged;
            singletonInstance.onCurrentWaveChanged += HandleCurrentWaveChanged;
            singletonInstance.onCurrentWaveGapChanged += HandleCurrentWaveGapChanged;
        }

        protected override void OnSingletonDisconnected()
        {
            if (singletonInstance == null) return;
            singletonInstance.onWavesInitialized -= HandleWavesInitialized;
            singletonInstance.onTotalWavesChanged -= HandleTotalWavesChanged;
            singletonInstance.onCurrentWaveChanged -= HandleCurrentWaveChanged;
            singletonInstance.onCurrentWaveGapChanged -= HandleCurrentWaveGapChanged;
        }

        private void HandleWavesInitialized()
        {
            RefreshDropdownOptions();
            RefreshCurrentWaveUI();
        }

        private void HandleTotalWavesChanged(int total)
        {
            RefreshDropdownOptions();
        }

        private void HandleCurrentWaveChanged(int index)
        {
            RefreshCurrentWaveUI();
        }

        private void HandleCurrentWaveGapChanged(int gap)
        {
            if (currentWaveGapInput != null) currentWaveGapInput.text = gap.ToString();
        }

        private void OnTotalWavesChanged(string text)
        {
            if (!int.TryParse(text, out var value)) value = 0;
            if (MapWaveEditManager.Instance == null) return;

            MapWaveEditManager.Instance.SetTotalWaves(value);
            RefreshDropdownOptions();
            RefreshCurrentWaveUI();
        }

        private void OnWaveIndexChanged(int index)
        {
            if (MapWaveEditManager.Instance == null) return;

            // 在切换前，先把当前场景中的敌人写回当前波次，并自动保存一次
            MapWaveEditManager.Instance.ApplySceneEnemiesToCurrentWave();
            SaveCurrentMap();

            MapWaveEditManager.Instance.SetCurrentWaveIndex(index);
            RefreshCurrentWaveUI();
            RefreshSceneByCurrentWave();
        }

        private void OnWaveGapChanged(string text)
        {
            if (!int.TryParse(text, out var value)) value = 0;
            if (MapWaveEditManager.Instance == null) return;
            MapWaveEditManager.Instance.SetCurrentWaveGap(value);
        }

        private void RefreshDropdownOptions()
        {
            if (MapWaveEditManager.Instance == null || waveIndexDropdown == null || totalWavesInput == null) return;

            var total = MapWaveEditManager.Instance.TotalWaves;
            totalWavesInput.text = total.ToString();

            waveIndexDropdown.ClearOptions();
            for (var i = 0; i < total; i++) waveIndexDropdown.options.Add(new TMP_Dropdown.OptionData($"波次 {i + 1}"));
            waveIndexDropdown.value = Mathf.Clamp(MapWaveEditManager.Instance.CurrentWaveIndex, 0, Mathf.Max(0, total - 1));
            waveIndexDropdown.RefreshShownValue();
        }

        private void RefreshCurrentWaveUI()
        {
            if (MapWaveEditManager.Instance == null) return;
            if (currentWaveGapInput != null) currentWaveGapInput.text = MapWaveEditManager.Instance.GetCurrentWaveGap().ToString();
        }

        private void RefreshSceneByCurrentWave()
        {
            if (MapWaveEditManager.Instance != null) MapWaveEditManager.Instance.OverwriteSceneEnemiesFromCurrentWave();
        }

        // 地图保存按钮调用：将场景中敌人写回到当前波次
        public void ApplySceneToCurrentWaveBeforeSave()
        {
            if (MapWaveEditManager.Instance != null) MapWaveEditManager.Instance.ApplySceneEnemiesToCurrentWave();
        }

        // 地图保存完成后（可选）同步总波次数/数据
        public void OnMapSaved()
        {
            InitializeFromCurrentMap();
            RefreshDropdownOptions();
            RefreshCurrentWaveUI();
        }

        private void SaveCurrentMap()
        {
            if (MapStorageManager.Instance == null) return;
            var data = MapStorageManager.Instance.GetCurrentMapData();
            if (data == null || string.IsNullOrEmpty(data.mapName)) return;

            // 直接保存到当前地图名
            MapStorageManager.Instance.SaveMap(data.mapName);
        }
    }
}


