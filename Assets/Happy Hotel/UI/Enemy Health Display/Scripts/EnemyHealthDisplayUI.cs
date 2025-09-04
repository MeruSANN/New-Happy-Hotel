using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.ValueProcessing.Components;
using TMPro;
using UnityEngine;

namespace HappyHotel.UI
{
    // Enemy血量显示UI，只显示当前血量值
    public class EnemyHealthDisplayUI : MonoBehaviour
    {
        [Header("血量显示")] [SerializeField] private TMP_Text healthText; // 血量文本 (仅显示当前值)
        [Header("护甲显示")] [SerializeField] private TMP_Text armorText; // 护甲文本
        [SerializeField] private GameObject armorContainer; // 护甲容器，当护甲值为0时隐藏

        // 组件引用
        private BehaviorComponentContainer container;
        private HitPointValueComponent hitPointComponent;
        private ArmorValueComponent armorComponent;

        private void Start()
        {
            InitializeHealthComponent();
            InitializeArmorComponent();
        }

        private void OnDestroy()
        {
            // 取消事件监听
            if (hitPointComponent != null)
                hitPointComponent.HitPointValue.onValueChanged.RemoveListener(OnHealthChanged);
            if (armorComponent != null)
                armorComponent.ArmorValue.onValueChanged.RemoveListener(OnArmorChanged);
        }

        // 在编辑器中验证UI组件是否已分配
        private void OnValidate()
        {
            if (healthText == null)
            {
                healthText = GetComponentInChildren<TMP_Text>();
                if (healthText == null) Debug.LogWarning("请将TMP_Text组件分配到EnemyHealthDisplayUI的healthText字段。", this);
            }
            
            if (armorText == null)
            {
                armorText = GetComponentInChildren<TMP_Text>();
                if (armorText == null) Debug.LogWarning("请将TMP_Text组件分配到EnemyHealthDisplayUI的armorText字段。", this);
            }
        }

        // 设置相关的BehaviorComponentContainer
        public void SetRelatedContainer(BehaviorComponentContainer componentContainer)
        {
            container = componentContainer;
            InitializeHealthComponent();
            InitializeArmorComponent();
        }

        private void InitializeHealthComponent()
        {
            if (!container) return;

            // 获取HealthComponent
            hitPointComponent = container.GetBehaviorComponent<HitPointValueComponent>();
            if (hitPointComponent != null)
            {
                // 注册事件监听
                hitPointComponent.HitPointValue.onValueChanged.AddListener(OnHealthChanged);
                // 初始化显示
                UpdateHealthDisplay();
            }
            else
            {
                Debug.LogWarning("EnemyHealthDisplayUI: 无法获取HitPointValueComponent");
            }
        }

        private void InitializeArmorComponent()
        {
            if (!container) return;

            // 获取ArmorComponent
            armorComponent = container.GetBehaviorComponent<ArmorValueComponent>();
            if (armorComponent != null)
            {
                // 注册事件监听
                armorComponent.ArmorValue.onValueChanged.AddListener(OnArmorChanged);
                // 初始化显示
                UpdateArmorDisplay();
            }
            else
            {
                Debug.LogWarning("EnemyHealthDisplayUI: 无法获取ArmorValueComponent");
            }
        }

        // 血量变化事件处理
        private void OnHealthChanged(int currentHealth)
        {
            UpdateHealthDisplay();
        }

        // 护甲变化事件处理
        private void OnArmorChanged(int currentArmor)
        {
            UpdateArmorDisplay();
        }

        // 更新血量显示
        private void UpdateHealthDisplay()
        {
            if (hitPointComponent == null || healthText == null) return;

            var currentHealth = hitPointComponent.CurrentHitPoint;
            healthText.text = currentHealth.ToString();
        }

        // 更新护甲显示
        private void UpdateArmorDisplay()
        {
            if (armorComponent == null) return;

            var currentArmor = armorComponent.CurrentArmor;
            
            // 更新护甲文本显示
            if (armorText != null)
            {
                armorText.text = currentArmor.ToString();
            }
            
            // 根据护甲值控制护甲容器的显示/隐藏
            if (armorContainer != null)
            {
                armorContainer.SetActive(currentArmor > 0);
            }
        }
    }
}