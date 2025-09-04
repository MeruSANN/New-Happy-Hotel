using System;
using HappyHotel.Core.Singleton;
using UnityEngine;

namespace HappyHotel.GameManager
{
    // 统一的配置提供者，跨场景常驻，负责加载/缓存GameConfig
    [ManagedSingleton(true)]
    public class ConfigProvider : SingletonBase<ConfigProvider>
    {
        [SerializeField] [Tooltip("GameConfig资源路径（Resources相对路径，无扩展名）")]
        private string configResourcePath = "GameConfig";

        private GameConfig cachedConfig;

        protected override void OnSingletonAwake()
        {
            base.OnSingletonAwake();
            EnsureLoaded();
        }

        // 获取配置（如未加载则尝试加载并缓存）
        public GameConfig GetGameConfig()
        {
            if (cachedConfig == null) EnsureLoaded();
            return cachedConfig;
        }

        // 重新加载配置
        public void ReloadConfig()
        {
            LoadInternal();
            Debug.Log(cachedConfig != null
                ? $"[ConfigProvider] ReloadConfig 成功，路径={configResourcePath}"
                : $"[ConfigProvider] ReloadConfig 失败，路径={configResourcePath}");
        }

        // 确保已加载
        public void EnsureLoaded()
        {
            if (cachedConfig == null)
            {
                LoadInternal();
                Debug.Log(cachedConfig != null
                    ? $"[ConfigProvider] EnsureLoaded 成功，路径={configResourcePath}"
                    : $"[ConfigProvider] EnsureLoaded 失败，路径={configResourcePath}");
            }
        }

        // 设置资源路径
        public void SetResourcePath(string path)
        {
            configResourcePath = path;
            Debug.Log($"[ConfigProvider] 设置配置文件路径: {path}");
        }

        // 获取资源路径
        public string GetResourcePath()
        {
            return configResourcePath;
        }

        private void LoadInternal()
        {
            try
            {
                cachedConfig = Resources.Load<GameConfig>(configResourcePath);
                if (cachedConfig == null)
                {
                    // 兼容可能的其他资源路径
                    string[] possiblePaths =
                    {
                        "GameConfig",
                        "Happy Hotel/Game Manager/GameConfig",
                        "Game Manager/GameConfig"
                    };
                    foreach (var p in possiblePaths)
                    {
                        cachedConfig = Resources.Load<GameConfig>(p);
                        if (cachedConfig != null)
                        {
                            configResourcePath = p;
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                cachedConfig = null;
                Debug.LogError($"[ConfigProvider] 加载配置时异常: {e.Message}");
            }
        }
    }
}