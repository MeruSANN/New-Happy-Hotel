using TMPro;
using UnityEngine;

namespace HappyHotel.Map
{
    public class MapSizeController : MonoBehaviour
    {
        [Header("UI组件")] public TMP_Text mapSizeText;

        public TMP_InputField widthInputField;
        public TMP_InputField heightInputField;

        private void Start()
        {
            // 初始化时读取当前地图尺寸并显示
            UpdateMapSizeDisplay();

            // 订阅地图尺寸改变事件
            if (MapManager.Instance != null) MapManager.Instance.onMapSizeChanged += OnMapSizeChanged;
        }

        private void OnDestroy()
        {
            // 取消订阅事件
            if (MapManager.Instance != null) MapManager.Instance.onMapSizeChanged -= OnMapSizeChanged;
        }

        // 更新地图尺寸显示
        private void UpdateMapSizeDisplay()
        {
            if (MapManager.Instance != null && mapSizeText != null)
            {
                var mapSize = MapManager.Instance.GetMapSize();
                mapSizeText.text = $"Map Size: {mapSize.x} x {mapSize.y}";
            }
        }

        // 地图尺寸改变事件回调
        private void OnMapSizeChanged(Vector2Int newSize)
        {
            UpdateMapSizeDisplay();
        }

        // 更新地图尺寸（由外部Button调用）
        public void UpdateMapSize()
        {
            if (MapManager.Instance == null)
            {
                Debug.LogError("MapManager实例不存在！");
                return;
            }

            // 获取输入框中的值
            if (int.TryParse(widthInputField.text, out var newWidth) &&
                int.TryParse(heightInputField.text, out var newHeight))
            {
                // 验证输入值的有效性
                if (newWidth > 0 && newHeight > 0)
                {
                    // 更新地图尺寸
                    MapManager.Instance.SetSize(newWidth, newHeight);
                    Debug.Log($"地图尺寸已更新为: {newWidth} x {newHeight}");
                }
                else
                {
                    Debug.LogWarning("地图尺寸必须大于0！");
                }
            }
            else
            {
                Debug.LogWarning("请输入有效的数字！");
            }
        }
    }
}