using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace HappyHotel.Map
{
    public class MapBoundaryManager : MonoBehaviour
    {
        [Header("边界设置")] [SerializeField] private TileBase boundaryTile;

        [SerializeField] private bool enableBoundary = true;
        private Tilemap boundaryTilemap;
        private Vector2Int currentMapSize;


        private Grid grid;
        private bool isUpdatingBoundary; // 防止重复更新


        private void Start()
        {
            // 查找Grid和边界Tilemap
            grid = GameObject.FindWithTag("Grid")?.GetComponent<Grid>();
            if (grid == null)
            {
                Debug.LogError("MapBoundaryManager: 找不到Grid组件!");
                return;
            }

            // 查找或创建边界Tilemap
            SetupBoundaryTilemap();

            // 订阅地图大小改变事件
            if (MapManager.Instance)
            {
                MapManager.Instance.onMapSizeChanged += MapSizeChanged;

                // 延迟初始化，确保所有组件都已准备就绪
                StartCoroutine(DelayedInitialization());
            }
        }

        private void OnDestroy()
        {
            // 取消订阅事件
            if (MapManager.Instance)
                MapManager.Instance.onMapSizeChanged -= MapSizeChanged;
        }

        private IEnumerator DelayedInitialization()
        {
            // 等待一帧，确保所有组件初始化完成
            yield return null;

            if (MapManager.Instance)
            {
                currentMapSize = MapManager.Instance.GetMapSize();
                StartCoroutine(UpdateBoundaryCoroutine());
            }
        }

        private void SetupBoundaryTilemap()
        {
            // 查找现有的边界Tilemap
            var boundaryObject = GameObject.FindWithTag("BoundaryTilemap");

            if (!boundaryObject)
                Debug.LogError("MapBoundaryManager: 无法获取BoundaryTilemap!");
            else
                boundaryTilemap = boundaryObject.GetComponent<Tilemap>();
        }

        private void MapSizeChanged(Vector2Int newSize)
        {
            currentMapSize = newSize;

            boundaryTilemap.ClearAllTiles();

            // 使用协程来避免渲染更新冲突
            if (!isUpdatingBoundary) StartCoroutine(UpdateBoundaryCoroutine());
        }

        private void UpdateBoundary()
        {
            // 使用协程版本替代直接调用
            if (!isUpdatingBoundary) StartCoroutine(UpdateBoundaryCoroutine());
        }

        private IEnumerator UpdateBoundaryCoroutine()
        {
            if (!enableBoundary || !boundaryTilemap || !MapManager.Instance || isUpdatingBoundary)
                yield break;

            isUpdatingBoundary = true;

            // 等待当前帧结束
            yield return new WaitForEndOfFrame();

            try
            {
                // 获取边界Tile
                if (!boundaryTile)
                {
                    Debug.LogWarning("MapBoundaryManager: 无法获取边界Tile，跳过边界更新");
                    yield break;
                }

                // 先更新MapManager的视觉地图
                if (MapManager.Instance) MapManager.Instance.UpdateVisualMap();

                // 等待一帧确保地图更新完成
                yield return null;

                // 在地图周围添加一圈边界
                // 上边界和下边界
                for (var x = -1; x <= currentMapSize.x; x++)
                {
                    // 上边界
                    var topPos = new Vector3Int(x, currentMapSize.y, 0);
                    boundaryTilemap.SetTile(topPos, boundaryTile);

                    // 下边界
                    var bottomPos = new Vector3Int(x, -1, 0);
                    boundaryTilemap.SetTile(bottomPos, boundaryTile);
                }

                // 左边界和右边界
                for (var y = -1; y <= currentMapSize.y; y++)
                {
                    // 左边界
                    var leftPos = new Vector3Int(-1, y, 0);
                    boundaryTilemap.SetTile(leftPos, boundaryTile);

                    // 右边界
                    var rightPos = new Vector3Int(currentMapSize.x, y, 0);
                    boundaryTilemap.SetTile(rightPos, boundaryTile);
                }

                // 强制刷新Tilemap
                boundaryTilemap.CompressBounds();
            }
            finally
            {
                isUpdatingBoundary = false;
            }
        }


        // 公共方法：手动更新边界
        public void RefreshBoundary()
        {
            if (MapManager.Instance)
            {
                currentMapSize = MapManager.Instance.GetMapSize();
                if (!isUpdatingBoundary) StartCoroutine(UpdateBoundaryCoroutine());
            }
        }


        // 公共方法：设置边界Tile类型
        public void SetBoundaryTileType(TileBase tile)
        {
            boundaryTile = tile;
            if (!isUpdatingBoundary) StartCoroutine(UpdateBoundaryCoroutine());
        }

        // 公共方法：启用/禁用边界
        public void SetBoundaryEnabled(bool enabled)
        {
            enableBoundary = enabled;
            if (enabled)
            {
                if (!isUpdatingBoundary) StartCoroutine(UpdateBoundaryCoroutine());
            }
            else if (boundaryTilemap != null)
            {
                StartCoroutine(ClearBoundaryCoroutine());
            }
        }

        private IEnumerator ClearBoundaryCoroutine()
        {
            yield return new WaitForEndOfFrame();

            if (boundaryTilemap != null)
            {
                // 只清除边界区域的Tile，不影响地图内容
                for (var x = -1; x <= currentMapSize.x; x++)
                {
                    var topPos = new Vector3Int(x, currentMapSize.y, 0);
                    var bottomPos = new Vector3Int(x, -1, 0);
                    boundaryTilemap.SetTile(topPos, null);
                    boundaryTilemap.SetTile(bottomPos, null);
                }

                for (var y = -1; y <= currentMapSize.y; y++)
                {
                    var leftPos = new Vector3Int(-1, y, 0);
                    var rightPos = new Vector3Int(currentMapSize.x, y, 0);
                    boundaryTilemap.SetTile(leftPos, null);
                    boundaryTilemap.SetTile(rightPos, null);
                }

                boundaryTilemap.CompressBounds();
            }
        }
    }
}