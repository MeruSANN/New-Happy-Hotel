using HappyHotel.Enemy;
using UnityEngine;

namespace HappyHotel.UI.HoverDisplay.EnemyHover
{
    // 敌人悬停接收器，专门用于检测敌人对象的悬停事件
    public class EnemyHoverReceiver : HoverDisplayReceiver
    {
        private EnemyBase enemyComponent;
        private EnemyHoverDisplayUI typedTargetUI;

        private void Start()
        {
            enemyComponent = GetComponent<EnemyBase>();
            typedTargetUI = targetUI as EnemyHoverDisplayUI;
            if (typedTargetUI == null)
            {
                typedTargetUI = FindObjectOfType<EnemyHoverDisplayUI>();
                if (typedTargetUI != null) targetUI = typedTargetUI;
            }

            if (enemyComponent == null) Debug.LogWarning($"GameObject {gameObject.name} 上没有找到EnemyBase组件，悬停功能可能无法正常工作");
        }

        protected override HoverDisplayData CreateHoverData()
        {
            if (enemyComponent == null) return null;
            var worldPosition = transform.position;
            Vector2 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
            return new EnemyHoverData(enemyComponent, worldPosition, screenPosition);
        }
    }
}