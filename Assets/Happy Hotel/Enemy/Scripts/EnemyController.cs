using System.Collections.Generic;
using HappyHotel.Core.Grid;
using HappyHotel.Core.Grid.Components;
using HappyHotel.Core.Registry;
using HappyHotel.Core.Singleton;
using UnityEngine;

namespace HappyHotel.Enemy
{
    // 敌人控制器，负责敌人的创建和位置管理业务逻辑
    [ManagedSingleton(SceneLoadMode.Exclude, "ShopScene", "MainMenu")]
    [SingletonInitializationDependency(typeof(GridObjectManager))]
    public class EnemyController : SingletonBase<EnemyController>
    {
        // 测试用：一键清除敌人功能开关
        [SerializeField] private bool enableTestClearEnemies = true;

        // 测试用：一键清除敌人的按键
        [SerializeField] private KeyCode testClearEnemiesKey = KeyCode.F10;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void Update()
        {
            // 测试用：一键清除敌人
            if (!enableTestClearEnemies) return;
            if (Input.GetKeyDown(testClearEnemiesKey))
            {
                ClearAllEnemies();
                Debug.Log("[EnemyController] 测试快捷键触发：已清除所有敌人");
            }
        }
#endif

        protected override void OnSingletonAwake()
        {
            // 确保GridObjectManager已初始化
            if (GridObjectManager.Instance == null) Debug.LogError("GridObjectManager未初始化，敌人控制可能无法正常工作");
        }

        // 创建敌人
        public EnemyBase CreateEnemy(EnemyTypeId typeId, Vector2Int position)
        {
            var enemy = EnemyManager.Instance.Create(typeId);
            if (enemy)
            {
                // 设置敌人位置
                enemy.transform.SetParent(transform);
                enemy.GetBehaviorComponent<GridObjectComponent>().MoveTo(position);
            }

            return enemy;
        }

        // 便捷方法：使用字符串创建敌人
        public EnemyBase CreateEnemy(string typeId, Vector2Int position)
        {
            var enemyTypeId = TypeId.Create<EnemyTypeId>(typeId);
            return CreateEnemy(enemyTypeId, position);
        }

        public EnemyBase GetEnemyAt(Vector2Int position)
        {
            if (GridObjectManager.Instance == null)
            {
                Debug.LogWarning("GridObjectManager未初始化，无法获取敌人");
                return null;
            }

            // 查找敌人
            var enemies = GridObjectManager.Instance.GetObjectsOfTypeAt<EnemyBase>(position);
            foreach (var enemy in enemies) return enemy;

            return null;
        }

        public List<EnemyBase> GetAllEnemiesAt(Vector2Int position)
        {
            if (GridObjectManager.Instance == null)
            {
                Debug.LogWarning("GridObjectManager未初始化，无法获取敌人");
                return new List<EnemyBase>();
            }

            // 查找所有敌人
            var enemies = new List<EnemyBase>();
            var containers = GridObjectManager.Instance.GetObjectsOfTypeAt<EnemyBase>(position);
            foreach (var enemy in containers) enemies.Add(enemy);

            return enemies;
        }

        // 移除指定位置的敌人
        public void RemoveEnemy(Vector2Int position)
        {
            if (GridObjectManager.Instance == null)
            {
                Debug.LogError("GridObjectManager未初始化，无法移除敌人");
                return;
            }

            var enemies = GridObjectManager.Instance.GetObjectsOfTypeAt<EnemyBase>(position);
            var enemiesToRemove = new List<EnemyBase>(enemies);

            foreach (var enemy in enemiesToRemove)
                if (enemy != null && enemy.gameObject != null)
                {
                    EnemyManager.Instance.Remove(enemy);
                    Destroy(enemy.gameObject);
                }
        }

        // 获取所有敌人
        public List<EnemyBase> GetAllEnemies()
        {
            if (GridObjectManager.Instance == null)
            {
                Debug.LogWarning("GridObjectManager未初始化，无法获取敌人");
                return new List<EnemyBase>();
            }

            return GridObjectManager.Instance.GetObjectsOfType<EnemyBase>();
        }

        // 获取当前场景中所有敌人的数量
        public int GetCurrentEnemyCount()
        {
            if (GridObjectManager.Instance == null) return 0;

            return GridObjectManager.Instance.GetObjectsOfType<EnemyBase>().Count;
        }

        // 获取指定类型敌人的数量
        public int GetEnemyCountOfType(string enemyTypeId)
        {
            if (GridObjectManager.Instance == null) return 0;

            var typeId = TypeId.Create<EnemyTypeId>(enemyTypeId);
            var allEnemies = GridObjectManager.Instance.GetObjectsOfType<EnemyBase>();

            var count = 0;
            foreach (var enemy in allEnemies)
                if (enemy.TypeId.Equals(typeId))
                    count++;

            return count;
        }

        // 清除所有敌人
        public void ClearAllEnemies()
        {
            if (GridObjectManager.Instance == null)
            {
                Debug.LogError("GridObjectManager未初始化，无法清除敌人");
                return;
            }

            var enemies = GridObjectManager.Instance.GetObjectsOfType<EnemyBase>();

            // 使用 ToList() 避免在遍历时修改集合
            var enemiesToClear = new List<EnemyBase>(enemies);
            foreach (var enemy in enemiesToClear)
                if (enemy != null && enemy.gameObject != null)
                {
                    EnemyManager.Instance.Remove(enemy);
                    Destroy(enemy.gameObject);
                }
        }

        public void ClearAllEnemiesImmediate()
        {
            if (GridObjectManager.Instance == null)
            {
                Debug.LogError("GridObjectManager未初始化，无法清除敌人");
                return;
            }

            var enemies = GridObjectManager.Instance.GetObjectsOfType<EnemyBase>();

            // 使用 ToList() 避免在遍历时修改集合
            var enemiesToClear = new List<EnemyBase>(enemies);
            foreach (var enemy in enemiesToClear)
                if (enemy != null && enemy.gameObject != null)
                {
                    EnemyManager.Instance.Remove(enemy);
                    DestroyImmediate(enemy.gameObject);
                }
        }
    }
}