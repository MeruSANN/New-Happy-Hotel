using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.ValueProcessing.Components;
using HappyHotel.GameManager;
using HappyHotel.Shop;
using TMPro;
using UnityEngine;

namespace HappyHotel.UI
{
    // 统一的游戏状态显示UI，包含血量、护甲、费用、金币和攻击力显示
    public class GameStatusDisplayUI : MonoBehaviour
    {
        [Header("血量显示")] [SerializeField] private TMP_Text healthText; // 血量文本 (当前值/上限值)

        [Header("护甲显示")] [SerializeField] private TMP_Text armorText; // 护甲文本 (仅显示数值)

        [Header("费用显示")] [SerializeField] private TMP_Text costText; // 费用文本 (当前值/上限值)

        [Header("金币显示")] [SerializeField] private TMP_Text coinText; // 金币文本 (仅显示数值)

        [Header("攻击力显示")] [SerializeField] private TMP_Text attackText; // 攻击力文本 (仅显示数值)

        private ArmorValueComponent armorComponent;
        private AttackPowerComponent attackPowerComponent;

        // 自动查找MainCharacter的BehaviorComponentContainer

        // 组件引用
        private BehaviorComponentContainer container;
        private CostManager costManager;
        private HitPointValueComponent hitPointComponent;

        // 事件监听标记
        private bool isArmorListenerRegistered;

        // 金币变化检测
        private int lastMoneyAmount = -1;
        private ShopMoneyManager moneyManager;

        // 攻击力变化检测
        private int lastAttackPower = -1;

        private void Start()
        {
            InitializeComponents();
        }

        private void Update()
        {
            // 如果container为空，则自动查找MainCharacter
            if (!container)
            {
                AutoFindContainer();
                InitializeComponents();
            }

            // 检查护甲组件是否动态添加
            CheckForArmorComponent();

            // 检查金币变化
            CheckMoneyChange();

            // 检查攻击力变化
            CheckAttackPowerChange();
        }

        private void OnDestroy()
        {
            // 取消事件监听
            if (hitPointComponent != null)
                hitPointComponent.HitPointValue.onValueChanged.RemoveListener(OnHealthChanged);

            UnregisterArmorListener();

            if (costManager != null) CostManager.onCostChanged -= UpdateCostDisplay;
        }

        // 在编辑器中验证UI组件是否已分配
        private void OnValidate()
        {
            if (healthText == null)
            {
                healthText = GetComponentInChildren<TMP_Text>();
                if (healthText == null) Debug.LogWarning("请将TMP_Text组件分配到GameStatusDisplayUI的healthText字段。", this);
            }
        }

        // 自动查找MainCharacter的BehaviorComponentContainer
        private void AutoFindContainer()
        {
            var targetObject = GameObject.FindGameObjectWithTag("MainCharacter");
            if (targetObject != null)
            {
                container = targetObject.GetComponent<BehaviorComponentContainer>();
                if (container == null)
                    Debug.LogError("GameStatusDisplayUI: 在MainCharacter对象上未找到BehaviorComponentContainer组件");
            }
        }


        private void InitializeComponents()
        {
            InitializeHealthComponent();
            InitializeArmorComponent();
            InitializeAttackPowerComponent();
            InitializeCostManager();
            // 敏捷系统已移除
            InitializeMoneyManager();
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
                Debug.LogWarning("GameStatusDisplayUI: 无法获取HitPointValueComponent");
            }
        }

        private void InitializeArmorComponent()
        {
            if (!container) return;

            // 获取ArmorComponent
            var newArmorComponent = container.GetBehaviorComponent<ArmorValueComponent>();

            // 如果找到了新的护甲组件且与当前不同
            if (newArmorComponent != null && newArmorComponent != armorComponent)
            {
                // 先解除旧的监听
                UnregisterArmorListener();

                // 设置新的护甲组件
                armorComponent = newArmorComponent;

                // 注册事件监听
                armorComponent.ArmorValue.onValueChanged.AddListener(OnArmorChanged);
                isArmorListenerRegistered = true;

                // 初始化显示
                UpdateArmorDisplay();
            }
            else if (newArmorComponent == null && armorComponent != null)
            {
                // 护甲组件被移除了
                UnregisterArmorListener();
                armorComponent = null;
                UpdateArmorDisplay();
            }
            else if (newArmorComponent == null)
            {
                // 没有护甲组件时隐藏护甲显示
                UpdateArmorDisplay();
            }
        }

        private void InitializeAttackPowerComponent()
        {
            if (!container) return;

            // 获取AttackPowerComponent
            attackPowerComponent = container.GetBehaviorComponent<AttackPowerComponent>();
            if (attackPowerComponent != null)
            {
                // 初始化显示
                UpdateAttackPowerDisplay();
            }
            else
            {
                Debug.LogWarning("GameStatusDisplayUI: 无法获取AttackPowerComponent");
                UpdateAttackPowerDisplay();
            }
        }

        private void InitializeCostManager()
        {
            costManager = CostManager.Instance;
            if (costManager != null)
            {
                // 订阅费用变化事件
                CostManager.onCostChanged += UpdateCostDisplay;
                // 初始化显示
                UpdateCostDisplay(costManager.CurrentCost, costManager.MaxCost);
            }
            else
            {
                Debug.LogWarning("GameStatusDisplayUI: 无法获取CostManager实例");
            }
        }

        // 敏捷系统已移除

        private void InitializeMoneyManager()
        {
            moneyManager = ShopMoneyManager.Instance;
            if (moneyManager != null)
                // 初始化显示
                UpdateCoinDisplay();
            else
                Debug.LogWarning("GameStatusDisplayUI: 无法获取ShopMoneyManager实例");
        }

        // 检查护甲组件是否动态添加
        private void CheckForArmorComponent()
        {
            if (!container) return;

            // 如果当前没有护甲组件，尝试重新获取
            if (armorComponent == null)
            {
                var newArmorComponent = container.GetBehaviorComponent<ArmorValueComponent>();
                if (newArmorComponent != null) InitializeArmorComponent();
            }
        }

        // 检查攻击力组件是否动态添加
        private void CheckForAttackPowerComponent()
        {
            if (!container) return;

            // 如果当前没有攻击力组件，尝试重新获取
            if (attackPowerComponent == null)
            {
                var newAttackPowerComponent = container.GetBehaviorComponent<AttackPowerComponent>();
                if (newAttackPowerComponent != null) InitializeAttackPowerComponent();
            }
        }

        // 解除护甲监听
        private void UnregisterArmorListener()
        {
            if (armorComponent != null && isArmorListenerRegistered)
            {
                armorComponent.ArmorValue.onValueChanged.RemoveListener(OnArmorChanged);
                isArmorListenerRegistered = false;
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

            var maxHealth = hitPointComponent.MaxHitPoint;
            var currentHealth = hitPointComponent.CurrentHitPoint;

            healthText.text = $"{currentHealth}/{maxHealth}";
        }

        // 更新护甲显示
        private void UpdateArmorDisplay()
        {
            if (armorText == null) return;

            var currentArmor = armorComponent?.CurrentArmor ?? 0;

            // 始终显示护甲文本，没有护甲组件时显示0
            armorText.gameObject.SetActive(true);
            armorText.text = currentArmor.ToString();
        }

        // 更新攻击力显示
        private void UpdateAttackPowerDisplay()
        {
            if (attackText == null) return;

            var currentAttackPower = attackPowerComponent?.GetAttackPower() ?? 0;

            // 始终显示攻击力文本，没有攻击力组件时显示0
            attackText.gameObject.SetActive(true);
            attackText.text = currentAttackPower.ToString();
        }

        // 更新费用显示
        private void UpdateCostDisplay(int currentCost, int maxCost)
        {
            if (costText == null) return;

            costText.text = $"{currentCost}/{maxCost}";
        }

        // 敏捷系统已移除

        // 更新金币显示
        private void UpdateCoinDisplay()
        {
            if (coinText == null || moneyManager == null) return;

            coinText.text = moneyManager.CurrentMoney.ToString();
        }

        // 检查金币变化
        private void CheckMoneyChange()
        {
            if (moneyManager == null) return;

            var currentMoney = moneyManager.CurrentMoney;
            if (currentMoney != lastMoneyAmount)
            {
                lastMoneyAmount = currentMoney;
                UpdateCoinDisplay();
            }
        }

        // 检查攻击力变化
        private void CheckAttackPowerChange()
        {
            if (attackPowerComponent == null) return;

            var currentAttackPower = attackPowerComponent.GetAttackPower();
            if (currentAttackPower != lastAttackPower)
            {
                lastAttackPower = currentAttackPower;
                UpdateAttackPowerDisplay();
            }
        }

        // 手动更新金币显示（供外部调用）
        public void RefreshCoinDisplay()
        {
            UpdateCoinDisplay();
        }

        // 手动更新攻击力显示（供外部调用）
        public void RefreshAttackPowerDisplay()
        {
            UpdateAttackPowerDisplay();
        }
    }
}