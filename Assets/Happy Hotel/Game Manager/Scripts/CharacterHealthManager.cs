using System;
using System.Collections;
using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.Singleton;
using HappyHotel.Core.ValueProcessing.Components;
using UnityEngine;

namespace HappyHotel.GameManager
{
    // MainCharacter血量管理器 - 在地图重新加载和跨场景时保持血量
    [ManagedSingleton(true)]
    public class CharacterHealthManager : SingletonBase<CharacterHealthManager>
    {
        private HitPointValueComponent currentHealthComponent;

        // MainCharacter引用
        private GameObject currentMainCharacter;
        private bool hasHealthData;
        private int savedCurrentHealth = -1;

        // 保存的血量数据
        private int savedMaxHealth = -1;

        private void Update()
        {
            // 检查MainCharacter是否发生变化
            CheckMainCharacterStatus();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // 取消事件订阅
            LevelManager.onLevelChanged -= OnLevelChanged;

            // 解除当前角色的事件监听
            UnregisterCurrentCharacterEvents();
        }

        // 血量状态保存事件
        public static event Action<int, int> onHealthSaved;
        public static event Action<int, int> onHealthRestored;

        protected override void OnSingletonAwake()
        {
            base.OnSingletonAwake();

            // 订阅关卡加载事件，在关卡加载完成后恢复血量
            LevelManager.onLevelChanged += OnLevelChanged;

            Debug.Log("CharacterHealthManager 初始化完成");
        }

        // 检查MainCharacter状态
        private void CheckMainCharacterStatus()
        {
            var mainCharacter = GameObject.FindGameObjectWithTag("MainCharacter");

            // 如果MainCharacter发生变化
            if (mainCharacter != currentMainCharacter)
            {
                // 解除旧角色的事件监听
                UnregisterCurrentCharacterEvents();

                currentMainCharacter = mainCharacter;

                if (currentMainCharacter != null)
                {
                    // 获取血量组件
                    var behaviorContainer = currentMainCharacter.GetComponent<BehaviorComponentContainer>();
                    if (behaviorContainer != null)
                    {
                        currentHealthComponent = behaviorContainer.GetBehaviorComponent<HitPointValueComponent>();

                        if (currentHealthComponent != null)
                        {
                            // 如果有保存的血量数据，立即恢复
                            if (hasHealthData) RestoreHealth();

                            // 注册血量变化监听
                            RegisterCurrentCharacterEvents();
                        }
                    }
                }
                else
                {
                    currentHealthComponent = null;
                }
            }
        }

        // 注册当前角色的事件监听
        private void RegisterCurrentCharacterEvents()
        {
            if (currentHealthComponent != null)
            {
                currentHealthComponent.HitPointValue.onValueChanged.AddListener(OnHealthChanged);
                Debug.Log("CharacterHealthManager: 已注册MainCharacter血量变化监听");
            }
        }

        // 解除当前角色的事件监听
        private void UnregisterCurrentCharacterEvents()
        {
            if (currentHealthComponent != null)
            {
                currentHealthComponent.HitPointValue.onValueChanged.RemoveListener(OnHealthChanged);
                Debug.Log("CharacterHealthManager: 已解除MainCharacter血量变化监听");
            }
        }

        // 血量变化时的回调
        private void OnHealthChanged(int newValue)
        {
            // 自动保存血量状态
            SaveCurrentHealth();
        }

        // 关卡变化时的回调
        private void OnLevelChanged(string levelName)
        {
            Debug.Log($"CharacterHealthManager: 关卡变化 - {levelName}，准备恢复血量");

            // 延迟恢复血量，等待MainCharacter完全初始化
            StartCoroutine(DelayedHealthRestore());
        }

        // 延迟恢复血量
        private IEnumerator DelayedHealthRestore()
        {
            // 等待几帧，确保MainCharacter完全初始化
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            // 尝试恢复血量
            if (hasHealthData) RestoreHealth();
        }

        // 保存当前血量
        public void SaveCurrentHealth()
        {
            if (currentHealthComponent != null)
            {
                savedMaxHealth = currentHealthComponent.MaxHitPoint;
                savedCurrentHealth = currentHealthComponent.CurrentHitPoint;
                hasHealthData = true;

                onHealthSaved?.Invoke(savedMaxHealth, savedCurrentHealth);
                Debug.Log($"CharacterHealthManager: 保存血量 - 最大血量: {savedMaxHealth}, 当前血量: {savedCurrentHealth}");
            }
        }

        // 恢复血量
        public void RestoreHealth()
        {
            if (!hasHealthData)
            {
                Debug.LogWarning("CharacterHealthManager: 没有保存的血量数据");
                return;
            }

            if (currentHealthComponent == null)
            {
                Debug.LogWarning("CharacterHealthManager: 当前没有MainCharacter的血量组件");
                return;
            }

            // 设置血量
            currentHealthComponent.SetHitPoint(savedMaxHealth, savedCurrentHealth);

            onHealthRestored?.Invoke(savedMaxHealth, savedCurrentHealth);
            Debug.Log($"CharacterHealthManager: 恢复血量 - 最大血量: {savedMaxHealth}, 当前血量: {savedCurrentHealth}");
        }

        // 手动保存血量（供外部调用）
        public void ManualSaveHealth()
        {
            SaveCurrentHealth();
        }

        // 手动恢复血量（供外部调用）
        public void ManualRestoreHealth()
        {
            RestoreHealth();
        }

        // 清除保存的血量数据（新游戏时使用）
        public void ClearHealthData()
        {
            savedMaxHealth = -1;
            savedCurrentHealth = -1;
            hasHealthData = false;

            Debug.Log("CharacterHealthManager: 清除血量数据");
        }

        // 设置血量数据（供外部设置）
        public void SetHealthData(int maxHealth, int currentHealth)
        {
            savedMaxHealth = maxHealth;
            savedCurrentHealth = currentHealth;
            hasHealthData = true;

            Debug.Log($"CharacterHealthManager: 设置血量数据 - 最大血量: {maxHealth}, 当前血量: {currentHealth}");
        }

        // 获取保存的血量数据
        public (int maxHealth, int currentHealth, bool hasData) GetHealthData()
        {
            return (savedMaxHealth, savedCurrentHealth, hasHealthData);
        }

        // 检查是否有血量数据
        public bool HasHealthData()
        {
            return hasHealthData;
        }

        // 获取当前MainCharacter的血量组件
        public HitPointValueComponent GetCurrentHealthComponent()
        {
            return currentHealthComponent;
        }

        // 获取当前MainCharacter对象
        public GameObject GetCurrentMainCharacter()
        {
            return currentMainCharacter;
        }
    }
}