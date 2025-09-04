using System;
using System.Collections.Generic;
using HappyHotel.Core;
using HappyHotel.GameManager;
using UnityEngine;

namespace HappyHotel.Map.Data
{
    [Serializable]
    public class MapData
    {
        // 地图版本号
        public int editorVersion = 1;
        public string mapName;
        public Vector2Int mapSize;

        public List<SerializedTile> tiles;

        // 是否存在玩家角色
        public bool hasPlayer;

        // 角色初始位置
        public Vector2Int playerStartPosition;

        // 角色初始朝向
        public Direction playerStartDirection;

        // 装置信息列表
        public List<SerializedDevice> devices;

        // 敌人信息改为仅使用波次（waves）存储

        // 通关后是否进入商店
        public bool shouldEnterShop;

        // 波次总数（冗余，= waves.Count）
        public int totalWaves;

        // 波次配置列表（顺序生效）
        public List<WaveConfig> waves;

        // 新增：关卡类型
        public LevelType levelType = LevelType.Normal;

        // 新增：关卡难度
        public int levelDifficulty = 1;

        public MapData(string name, Vector2Int size)
        {
            mapName = name;
            mapSize = size;
            tiles = new List<SerializedTile>();
            devices = new List<SerializedDevice>();
            hasPlayer = false; // 默认没有玩家
            shouldEnterShop = false; // 默认不进入商店
            levelType = LevelType.Normal; // 默认为普通关卡
            levelDifficulty = 1; // 默认为难度1
            waves = new List<WaveConfig>();
            totalWaves = 0;
        }
    }

    [Serializable]
    public class SerializedTile
    {
        public int x;
        public int y;
        public TileType type;

        public SerializedTile(int x, int y, TileType type)
        {
            this.x = x;
            this.y = y;
            this.type = type;
        }
    }

    // 新增：用于序列化装置信息
    [Serializable]
    public class SerializedDevice
    {
        public Vector2Int position;

        public string deviceType; // 使用字符串存储装置种类

        // LevelExit Device的下一关分支编号（可选字段，仅对LevelExit类型有效）
        public int nextLevelBranchIndex;

        public SerializedDevice(Vector2Int position, string deviceType)
        {
            this.position = position;
            this.deviceType = deviceType;
            nextLevelBranchIndex = 0; // 默认值为0
        }

        // 带分支编号的构造函数
        public SerializedDevice(Vector2Int position, string deviceType, int nextLevelBranchIndex)
        {
            this.position = position;
            this.deviceType = deviceType;
            this.nextLevelBranchIndex = nextLevelBranchIndex;
        }
    }

    // 已移除：静态敌人序列化结构（统一使用waves）

    // 新增：波次配置
    [Serializable]
    public class WaveConfig
    {
        public int gapFromPreviousTurns; // 距离上一波的回合差（第1波忽略）
        public List<WaveEnemy> enemies; // 本波敌人清单

        public WaveConfig()
        {
            gapFromPreviousTurns = 0;
            enemies = new List<WaveEnemy>();
        }
    }

    // 新增：波次中的敌人条目
    [Serializable]
    public class WaveEnemy
    {
        public string enemyTypeId;
        public Vector2Int position;

        public WaveEnemy(string typeId, Vector2Int pos)
        {
            enemyTypeId = typeId;
            position = pos;
        }
    }
}