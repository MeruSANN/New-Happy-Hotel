using System.Collections;
using HappyHotel.GameManager;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HappyHotel.Map
{
    public class CameraMapSizeFitter : MonoBehaviour
    {
        [Header("摄像机设置")] [SerializeField] private Camera targetCamera;

        [SerializeField] private bool autoAdjustCamera = true;
        [SerializeField] private float cameraMargin = 1f; // 摄像机边距
        [SerializeField] private float minCameraSize = 5f; // 最小摄像机尺寸
        [SerializeField] private float maxCameraSize = 50f; // 最大摄像机尺寸
        private Vector2Int currentMapSize;

        // 协程管理
        private Coroutine delayedInitializationCoroutine;

        private Grid grid;

        // 组件销毁标记
        private bool isDestroyed;

        // 场景模式检测
        private bool isMapEditMode;
        private Coroutine mapUpdateCoroutine;

        private void Awake()
        {
            // 如果没有指定摄像机，使用主摄像机
            if (targetCamera == null) targetCamera = Camera.main;

            // 检测当前场景模式
            isMapEditMode = SceneManager.GetActiveScene().name == "MapEditScene";
        }

        private void Start()
        {
            // 查找Grid组件
            grid = GameObject.FindWithTag("Grid")?.GetComponent<Grid>();
            if (grid == null)
            {
                Debug.LogError("CameraManager: 找不到Grid组件!");
                return;
            }

            // 延迟初始化，等待所有单例管理器准备就绪
            delayedInitializationCoroutine = StartCoroutine(DelayedInitialization());
        }

        private void OnDestroy()
        {
            // 设置销毁标记
            isDestroyed = true;

            // 停止所有协程
            if (delayedInitializationCoroutine != null)
            {
                StopCoroutine(delayedInitializationCoroutine);
                delayedInitializationCoroutine = null;
            }

            if (mapUpdateCoroutine != null)
            {
                StopCoroutine(mapUpdateCoroutine);
                mapUpdateCoroutine = null;
            }

            // 取消订阅事件
            if (MapManager.Instance != null) MapManager.Instance.onMapSizeChanged -= OnMapSizeChanged;

            // 只在非编辑模式下取消订阅LevelManager事件
            if (!isMapEditMode) LevelManager.onLevelChanged -= OnLevelChanged;

            Debug.Log("CameraManager: 已清理所有协程和事件订阅");
        }

        private IEnumerator DelayedInitialization()
        {
            // 等待所有必需的组件准备就绪
            yield return StartCoroutine(WaitForRequiredComponents());

            // 检查对象是否已被销毁
            if (isDestroyed || this == null) yield break;

            // 订阅地图大小改变事件
            if (MapManager.Instance != null)
            {
                MapManager.Instance.onMapSizeChanged += OnMapSizeChanged;
                Debug.Log("CameraManager: 成功订阅MapManager事件");
            }

            // 根据场景模式订阅不同的事件
            if (!isMapEditMode)
            {
                // 在游戏场景中订阅关卡加载完成事件
                LevelManager.onLevelChanged += OnLevelChanged;
                Debug.Log("CameraManager: 成功订阅LevelManager事件");
            }
            else
            {
                Debug.Log("CameraManager: 地图编辑模式，跳过LevelManager事件订阅");
            }

            // 获取当前地图大小并更新摄像机
            if (MapManager.Instance != null)
            {
                currentMapSize = MapManager.Instance.GetMapSize();
                UpdateCamera();
            }

            Debug.Log($"CameraManager: 初始化完成，地图大小: {currentMapSize}, 编辑模式: {isMapEditMode}");
        }

        private IEnumerator WaitForRequiredComponents()
        {
            Debug.Log("CameraManager: 开始等待必需组件...");

            // 等待MapManager实例准备就绪
            while (MapManager.Instance == null && !isDestroyed && this != null) yield return null;

            // 检查对象是否已被销毁
            if (isDestroyed || this == null) yield break;

            Debug.Log("CameraManager: MapManager已准备就绪");

            // 根据场景模式决定是否等待LevelManager
            if (!isMapEditMode)
            {
                // 在游戏场景中等待LevelManager实例准备就绪
                while (LevelManager.Instance == null && !isDestroyed && this != null) yield return null;

                // 检查对象是否已被销毁
                if (isDestroyed || this == null) yield break;

                Debug.Log("CameraManager: LevelManager已准备就绪");

                // 检查LevelManager是否已完成初始化
                while (LevelManager.Instance != null && !LevelManager.Instance.IsInitialized && !isDestroyed &&
                       this != null) yield return null;

                // 检查对象是否已被销毁
                if (isDestroyed || this == null) yield break;

                Debug.Log("CameraManager: LevelManager已完成初始化");
            }
            else
            {
                Debug.Log("CameraManager: 地图编辑模式，跳过LevelManager等待");
            }

            Debug.Log("CameraManager: 所有必需组件已准备就绪");
        }

        private void OnMapSizeChanged(Vector2Int newSize)
        {
            // 检查对象是否已被销毁
            if (isDestroyed || this == null) return;

            currentMapSize = newSize;
            Debug.Log($"CameraManager: 地图大小改变为 {newSize}");
            UpdateCamera();
        }

        private void OnLevelChanged(string levelName)
        {
            // 检查对象是否已被销毁
            if (isDestroyed || this == null) return;

            Debug.Log($"CameraManager: 关卡改变为 {levelName}，等待地图更新完成");

            // 停止之前的地图更新协程
            if (mapUpdateCoroutine != null) StopCoroutine(mapUpdateCoroutine);

            // 关卡改变后等待地图更新完成再调整摄像机
            mapUpdateCoroutine = StartCoroutine(WaitForMapUpdateAndRefreshCamera());
        }

        private IEnumerator WaitForMapUpdateAndRefreshCamera()
        {
            // 等待地图管理器更新完成
            if (MapManager.Instance != null)
            {
                Vector2Int newMapSize;
                var lastMapSize = currentMapSize;

                // 等待地图大小真正改变或至少等待一帧确保地图更新完成
                do
                {
                    // 检查对象是否已被销毁
                    if (isDestroyed || this == null) yield break;

                    yield return null;

                    // 再次检查对象是否已被销毁
                    if (isDestroyed || this == null) yield break;

                    // 检查MapManager是否仍然存在
                    if (MapManager.Instance == null) yield break;

                    newMapSize = MapManager.Instance.GetMapSize();
                } while (newMapSize == lastMapSize && newMapSize == Vector2Int.zero && !isDestroyed && this != null);

                // 最终检查对象是否已被销毁
                if (isDestroyed || this == null) yield break;

                // 更新摄像机
                currentMapSize = newMapSize;
                UpdateCamera();
                Debug.Log($"CameraManager: 关卡加载后更新摄像机，地图大小: {currentMapSize}");
            }

            // 清理协程引用
            mapUpdateCoroutine = null;
        }

        private void UpdateCamera()
        {
            // 检查对象是否已被销毁
            if (isDestroyed || this == null) return;

            if (!autoAdjustCamera)
            {
                Debug.Log("CameraManager: 自动调整摄像机已禁用");
                return;
            }

            if (targetCamera == null)
            {
                Debug.LogWarning("CameraManager: 目标摄像机为空");
                return;
            }

            if (grid == null)
            {
                Debug.LogWarning("CameraManager: Grid组件为空");
                return;
            }

            // 计算地图中心位置（包括边界）
            var mapCenter = grid.CellToWorld(new Vector3Int(
                currentMapSize.x / 2,
                currentMapSize.y / 2,
                0
            ));

            // 调整摄像机位置到地图中心
            var cameraPos = targetCamera.transform.position;
            cameraPos.x = mapCenter.x;
            cameraPos.y = mapCenter.y;
            targetCamera.transform.position = cameraPos;

            Debug.Log($"CameraManager: 摄像机位置调整为 {cameraPos}，地图中心: {mapCenter}");

            // 计算合适的摄像机尺寸
            if (targetCamera.orthographic)
            {
                // 计算需要显示的区域大小（地图 + 边界 + 边距）
                float mapWidth = currentMapSize.x + 2; // +2 for boundary
                float mapHeight = currentMapSize.y + 2; // +2 for boundary

                // 考虑摄像机的宽高比
                var aspectRatio = (float)Screen.width / Screen.height;

                // 计算需要的摄像机尺寸
                var requiredSizeForHeight = (mapHeight + cameraMargin * 2) / 2f;
                var requiredSizeForWidth = (mapWidth + cameraMargin * 2) / (2f * aspectRatio);

                // 取较大的尺寸以确保完整显示
                var requiredSize = Mathf.Max(requiredSizeForHeight, requiredSizeForWidth);

                // 限制在最小和最大尺寸之间
                var oldSize = targetCamera.orthographicSize;
                requiredSize = Mathf.Clamp(requiredSize, minCameraSize, maxCameraSize);

                targetCamera.orthographicSize = requiredSize;

                Debug.Log($"CameraManager: 摄像机尺寸从 {oldSize} 调整为 {requiredSize}，地图大小: {currentMapSize}");
            }
        }

        // 公共方法：手动更新摄像机
        public void RefreshCamera()
        {
            // 检查对象是否已被销毁
            if (isDestroyed || this == null) return;

            if (MapManager.Instance != null)
            {
                currentMapSize = MapManager.Instance.GetMapSize();
                Debug.Log($"CameraManager: 手动刷新摄像机，地图大小: {currentMapSize}");
                UpdateCamera();
            }
            else
            {
                Debug.LogWarning("CameraManager: MapManager实例为空，无法刷新摄像机");
            }
        }

        // 公共方法：启用/禁用自动摄像机调整
        public void SetAutoAdjustCamera(bool enabled)
        {
            // 检查对象是否已被销毁
            if (isDestroyed || this == null) return;

            autoAdjustCamera = enabled;
            if (enabled) UpdateCamera();
        }

        // 公共方法：设置摄像机边距
        public void SetCameraMargin(float margin)
        {
            // 检查对象是否已被销毁
            if (isDestroyed || this == null) return;

            cameraMargin = margin;
            if (autoAdjustCamera) UpdateCamera();
        }

        // 公共方法：设置摄像机尺寸范围
        public void SetCameraSizeRange(float minSize, float maxSize)
        {
            // 检查对象是否已被销毁
            if (isDestroyed || this == null) return;

            minCameraSize = minSize;
            maxCameraSize = maxSize;
            if (autoAdjustCamera) UpdateCamera();
        }

        // 公共方法：设置目标摄像机
        public void SetTargetCamera(Camera camera)
        {
            // 检查对象是否已被销毁
            if (isDestroyed || this == null) return;

            targetCamera = camera;
            if (autoAdjustCamera) UpdateCamera();
        }

        // 公共方法：获取当前场景模式
        public bool IsMapEditMode()
        {
            return isMapEditMode;
        }
    }
}