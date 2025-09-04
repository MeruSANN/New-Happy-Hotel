using System.Collections.Generic;
using System.Linq;
using HappyHotel.Core.Grid;
using HappyHotel.Core.Grid.Components;
using HappyHotel.Core.Singleton;
using HappyHotel.GameManager;
using HappyHotel.Prop.Settings;
using UnityEngine;

namespace HappyHotel.Prop
{
    // 道具控制器，负责道具的放置、移除和管理业务逻辑
    [ManagedSingleton(SceneLoadMode.Exclude, "ShopScene", "MainMenu")]
    public class PropController : SingletonBase<PropController>
    {
        // 正在放置的Prop列表，用于锁定触发功能
        private readonly HashSet<PropBase> placingProps = new();

        protected override void OnDestroy()
        {
            base.OnDestroy();
            TurnManager.onEnemyTurnEnd -= OnTurnEnd;
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

            // 绘制网格中所有道具的位置
            Gizmos.color = Color.cyan;
            if (GridObjectManager.Instance)
            {
                var propContainers = GridObjectManager.Instance.GetObjectsOfType<PropBase>();
                foreach (var propContainer in propContainers)
                    if (propContainer)
                        Gizmos.DrawWireSphere(propContainer.transform.position, 0.1f);
            }
        }

        protected override void OnSingletonAwake()
        {
            base.OnSingletonAwake();
            TurnManager.onEnemyTurnEnd += OnTurnEnd;
        }

        private void OnTurnEnd(int turnNumber)
        {
            ClearCardPlacedProps();
        }

        public PropBase PlaceProp(Vector2Int position, PropTypeId propType, IPropSetting setting = null)
        {
            if (!GridObjectManager.Instance)
            {
                Debug.LogError("GridObjectManager未初始化，无法放置道具");
                return null;
            }

            // 使用PropManager创建道具
            var prop = PropManager.Instance.Create(propType, setting);
            if (prop)
            {
                // 将Prop添加到放置列表中，锁定触发功能
                placingProps.Add(prop);

                try
                {
                    // 设置道具位置
                    prop.transform.SetParent(transform);
                    prop.GetBehaviorComponent<GridObjectComponent>().MoveTo(position);

                    // 标记Prop为已初始化，允许触发
                    prop.MarkAsInitialized();

                    Debug.Log($"已放置道具 {prop.name} 到位置 {position}");
                }
                finally
                {
                    // 确保无论是否成功，都要从放置列表中移除
                    placingProps.Remove(prop);
                }
            }

            return prop;
        }

        // 检查Prop是否正在被放置
        public bool IsPropBeingPlaced(PropBase prop)
        {
            return placingProps.Contains(prop);
        }

        // 手动解锁Prop（用于测试或其他特殊情况）
        public void UnlockProp(PropBase prop)
        {
            placingProps.Remove(prop);
        }

        public void RemoveProp(Vector2Int position)
        {
            if (!GridObjectManager.Instance)
            {
                Debug.LogError("GridObjectManager未初始化，无法移除道具");
                return;
            }

            // 使用网格系统获取并移除道具
            var props = GridObjectManager.Instance.GetObjectsOfTypeAt<PropBase>(position);
            foreach (var prop in props.ToList())
            {
                PropManager.Instance.Remove(prop);
                Destroy(prop.gameObject);
                Debug.Log($"已移除位置 {position} 的道具");
            }
        }

        public PropBase GetPropAt(Vector2Int position)
        {
            if (!GridObjectManager.Instance)
            {
                Debug.LogWarning("GridObjectManager未初始化，无法获取道具");
                return null;
            }

            // 查找具有GridObjectComponent的道具
            var props = GridObjectManager.Instance.GetObjectsOfTypeAt<PropBase>(position);
            foreach (var prop in props) return prop;

            return null;
        }

        public List<PropBase> GetAllPropsAt(Vector2Int position)
        {
            if (!GridObjectManager.Instance)
            {
                Debug.LogWarning("GridObjectManager未初始化，无法获取道具");
                return new List<PropBase>();
            }

            // 查找具有GridObjectComponent的所有道具
            var props = new List<PropBase>();
            var containers = GridObjectManager.Instance.GetObjectsOfTypeAt<PropBase>(position);
            foreach (var prop in containers) props.Add(prop);

            return props;
        }

        public List<T> GetAllPropsOfType<T>() where T : PropBase
        {
            if (!GridObjectManager.Instance)
            {
                Debug.LogWarning("GridObjectManager未初始化，无法获取道具");
                return new List<T>();
            }

            return GridObjectManager.Instance.GetObjectsOfType<T>();
        }

        // 获取场上所有装备类道具的数量
        public int GetEquipmentPropCount()
        {
            if (!GridObjectManager.Instance)
            {
                Debug.LogWarning("GridObjectManager未初始化，无法获取道具数量");
                return 0;
            }

            var allProps = GridObjectManager.Instance.GetObjectsOfType<PropBase>();
            return allProps.Count(prop => prop is EquipmentPropBase);
        }

        public void ClearCardPlacedProps()
        {
            if (!GridObjectManager.Instance)
            {
                Debug.LogError("GridObjectManager未初始化，无法清除道具");
                return;
            }

            // 查找所有道具并清除被卡牌放置的道具
            var containers = GridObjectManager.Instance.GetObjectsOfType<PropBase>();
            int clearedCount = 0;
            foreach (var propContainer in containers)
            {
                // 只清除被卡牌放置的道具
                if (propContainer.GetPlacedByCard() != null)
                {
                    PropManager.Instance.Remove(propContainer);
                    Destroy(propContainer.gameObject);
                    clearedCount++;
                }
            }

            Debug.Log($"已清除 {clearedCount} 个被卡牌放置的道具");
        }

        // 敏捷系统已移除

        // 清空所有道具
        public void ClearAllProps()
        {
            if (!GridObjectManager.Instance)
            {
                Debug.LogError("GridObjectManager未初始化，无法清除道具");
                return;
            }

            // 查找所有道具并清除
            var containers = GridObjectManager.Instance.GetObjectsOfType<PropBase>();
            int clearedCount = 0;
            foreach (var propContainer in containers)
            {
                PropManager.Instance.Remove(propContainer);
                Destroy(propContainer.gameObject);
                clearedCount++;
            }

            Debug.Log($"已清除所有 {clearedCount} 个道具");
        }
    }
}