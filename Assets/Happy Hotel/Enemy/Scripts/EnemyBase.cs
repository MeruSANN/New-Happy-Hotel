using HappyHotel.Buff.Components;
using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.Grid.Components;
using HappyHotel.Core.Registry;
using HappyHotel.Core.ValueProcessing.Components;
using HappyHotel.Intent;
using HappyHotel.Intent.Components;
using HappyHotel.Enemy.Templates;
using UnityEngine;
using HappyHotel.Core.Combat;

namespace HappyHotel.Enemy
{
    [AutoInitComponent(typeof(HitPointValueComponent))]
    [AutoInitComponent(typeof(GridObjectComponent))]
    [AutoInitComponent(typeof(BuffContainer))]
    [AutoInitComponent(typeof(TurnEndIntentExecutorComponent))]
    [AutoInitComponent(typeof(AttackPowerComponent))]
    [AutoInitComponent(typeof(ArmorValueComponent))]
    [AutoInitComponent(typeof(AttackEventHub))]
    public abstract class EnemyBase : BehaviorComponentContainer, ITypeIdSettable<EnemyTypeId>
    {
        // 敌人的基本属性
        protected EnemyTemplate template;

        // 组件引用
        protected HitPointValueComponent hitPointComponent;
        protected TurnEndIntentExecutorComponent turnEndIntentExecutorComponent;
        protected AttackPowerComponent attackPowerComponent;

        // 敌人的类型ID
        public EnemyTypeId TypeId { get; private set; }

        // 公开访问模板的属性
        public EnemyTemplate Template => template;

        protected override void Awake()
        {
            // 添加标签
            AddTag("Enemy");
            
            base.Awake();

            // 获取生命值组件
            hitPointComponent = GetBehaviorComponent<HitPointValueComponent>();
            hitPointComponent.HitPointValue.onDeath.AddListener(OnDeath);

            // 获取回合结束意图执行器
            turnEndIntentExecutorComponent = GetBehaviorComponent<TurnEndIntentExecutorComponent>();

            // 获取攻击力组件
            attackPowerComponent = GetBehaviorComponent<AttackPowerComponent>();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EnemyManager.Instance.Remove(this);
        }

        // 实现ITypeIdSettable接口
        public void SetTypeId(EnemyTypeId typeId)
        {
            TypeId = typeId;
        }

        public void SetTemplate(EnemyTemplate newTemplate)
        {
            template = newTemplate;
            hitPointComponent.SetHitPoint(template.baseHealth, template.baseHealth);
            
            // 配置回合结束意图执行器
            ConfigureTurnEndIntentExecutorComponent();
            
            GetComponent<SpriteRenderer>().sprite = newTemplate.enemySprite;
            OnTemplateSet();
        }

        // 当配置被设置时调用
        protected virtual void OnTemplateSet()
        {
        }

        // 配置回合结束意图执行器
        protected virtual void ConfigureTurnEndIntentExecutorComponent()
        {
            if (turnEndIntentExecutorComponent == null) return;

            if (attackPowerComponent != null)
            {
                attackPowerComponent.SetAttackPower(template.attackPower);
            }

            var plans = new System.Collections.Generic.List<TurnEndIntentExecutorComponent.IntentPlan>();
            if (template.intentSequence != null && template.intentSequence.Count > 0)
            {
                foreach (var item in template.intentSequence)
                {
                    if (string.IsNullOrEmpty(item.typeId)) continue;
                    var id = Core.Registry.TypeId.Create<IntentTypeId>(item.typeId);
                    plans.Add(new TurnEndIntentExecutorComponent.IntentPlan
                    {
                        TypeId = id,
                        Setting = item.setting
                    });
                }
            }

            if (plans.Count == 0)
            {
                var fallback = Core.Registry.TypeId.Create<IntentTypeId>("DamageMainCharacter");
                plans.Add(new TurnEndIntentExecutorComponent.IntentPlan { TypeId = fallback, Setting = null });
            }

            turnEndIntentExecutorComponent.SetSequence(plans);
            turnEndIntentExecutorComponent.SetLoop(true);
            turnEndIntentExecutorComponent.SetActive(true);

            Debug.Log($"敌人 {gameObject.name} 配置完成意图 - 攻击力: {template.attackPower}，意图数: {plans.Count}");
        }

        protected virtual void OnDeath()
        {
            Debug.Log($"敌人 {gameObject.name} 死亡");

            Destroy(gameObject);
        }

        // 敌人的显示/隐藏
        public virtual void Show()
        {
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }

        // 获取攻击力（用于调试或UI显示）
        public int GetAttackPower()
        {
            return attackPowerComponent?.GetAttackPower() ?? 0;
        }
    }
}