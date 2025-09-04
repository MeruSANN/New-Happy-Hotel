using System.Collections.Generic;
using System.Linq;
using HappyHotel.Core.Singleton;
using UnityEngine;

namespace HappyHotel.GameManager
{
    // 唯一物品管理器，负责跟踪已获得的唯一物品
    [ManagedSingleton(SceneLoadMode.Exclude, "MapEditScene", "MainMenu")]
    public class UniqueItemManager : SingletonBase<UniqueItemManager>
    {
        // 本地存储键名
        private const string UNIQUE_ITEMS_KEY = "ObtainedUniqueItems";

        // 已获得的唯一物品类型ID集合
        private readonly HashSet<string> obtainedUniqueItems = new();


        // 标记物品为已获得
        public void MarkItemAsObtained(string itemTypeId)
        {
            if (obtainedUniqueItems.Add(itemTypeId)) Debug.Log($"标记唯一物品为已获得: {itemTypeId}");
        }

        // 检查物品是否已被获得
        public bool IsItemObtained(string itemTypeId)
        {
            return obtainedUniqueItems.Contains(itemTypeId);
        }

        // 获取所有已获得的唯一物品类型ID
        public List<string> GetObtainedUniqueItems()
        {
            return obtainedUniqueItems.ToList();
        }

        // 清除已获得的唯一物品记录（用于新游戏或重置）
        public void ClearObtainedUniqueItems()
        {
            obtainedUniqueItems.Clear();
            PlayerPrefs.DeleteKey(UNIQUE_ITEMS_KEY);
            PlayerPrefs.Save();
            Debug.Log("已清除所有唯一物品获得记录");
        }
    }
}