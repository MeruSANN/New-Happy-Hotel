using HappyHotel.Core.Grid;
using UnityEngine;

namespace HappyHotel.Utils
{
    // 跟随鼠标显示目标格子的指示器
    public class TargetGridIndicator : MonoBehaviour
    {
        [Header("渲染组件")] [SerializeField] private SpriteRenderer spriteRenderer;

        [SerializeField] private Color validColor = new(0f, 1f, 0f, 0.35f);
        [SerializeField] private Color invalidColor = new(1f, 0f, 0f, 0.35f);

        private GridObjectManager gridManager;

        private void Awake()
        {
            gridManager = FindObjectOfType<GridObjectManager>();
            SetValid(false);
        }

        private void Update()
        {
            if (!gameObject.activeInHierarchy) return;
            if (Camera.main == null) return;

            var gridPos = GetMouseGridPosition();

            if (gridManager != null)
            {
                var worldCell = gridManager.GridToWorld(gridPos);
                transform.position = worldCell;
            }
            else
            {
                transform.position = new Vector3(gridPos.x, gridPos.y, 0f);
            }
        }

        public void Activate()
        {
            if (!gameObject.activeSelf) gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            if (gameObject.activeSelf) gameObject.SetActive(false);
        }

        public void SetValid(bool isValid)
        {
            if (spriteRenderer != null) spriteRenderer.color = isValid ? validColor : invalidColor;
        }

        private Vector2Int GetMouseGridPosition()
        {
            var mouseScreenPos = Input.mousePosition;

            if (Camera.main)
            {
                var mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
                mouseWorldPos.z = 0;

                Vector2Int gridPos;
                if (gridManager != null)
                    gridPos = gridManager.WorldToGrid(mouseWorldPos);
                else
                    gridPos = new Vector2Int(Mathf.RoundToInt(mouseWorldPos.x), Mathf.RoundToInt(mouseWorldPos.y));

                return gridPos;
            }

            return default;
        }
    }
}