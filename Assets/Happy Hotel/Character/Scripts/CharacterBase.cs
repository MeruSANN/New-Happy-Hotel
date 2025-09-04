using HappyHotel.Buff.Components;
using HappyHotel.Character.Components;
using HappyHotel.Character.Templates;
using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.Combat;
using HappyHotel.Core.Grid.Components;
using HappyHotel.Core.Registry;
using HappyHotel.Core.ValueProcessing.Components;
using HappyHotel.Prop.Components;
using HappyHotel.UI;
using UnityEngine;

namespace HappyHotel.Character
{
    [AutoInitComponent(typeof(HitPointValueComponent))]
    [AutoInitComponent(typeof(DirectionComponent))]
    [AutoInitComponent(typeof(PropTriggerComponent))]
    [AutoInitComponent(typeof(AutoMoveComponent))]
    [AutoInitComponent(typeof(GridObjectComponent))]
    [AutoInitComponent(typeof(BuffContainer))]
    [AutoInitComponent(typeof(AttackPowerComponent))]
    [AutoInitComponent(typeof(AttackComponent))]
    [AutoInitComponent(typeof(AttackEventHub))]
    public abstract class CharacterBase : BehaviorComponentContainer, ITypeIdSettable<CharacterTypeId>
    {
        protected CharacterTemplate template;

        // 组件引用
        protected HitPointValueComponent hitPointComponent;
        protected AttackPowerComponent attackPowerComponent;

        // 角色的类型ID
        public CharacterTypeId TypeId { get; private set; }

        protected override void Awake()
        {
            // 设置标签
            AddTag("Character");
            
            base.Awake();

            // 获取组件引用
            hitPointComponent = GetBehaviorComponent<HitPointValueComponent>();
            attackPowerComponent = GetBehaviorComponent<AttackPowerComponent>();

            // 设置事件监听
            hitPointComponent.HitPointValue.onDeath.AddListener(OnDeath);
            
            // 设置攻击组件的目标标签
            var attackComponent = GetBehaviorComponent<AttackComponent>();
            if (attackComponent != null)
            {
                attackComponent.SetTargetTags(new string[] { "Enemy" });
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // 解除事件监听
            CharacterManager.Instance.Remove(this);
        }

        // 实现ITypeIdSettable接口
        public void SetTypeId(CharacterTypeId typeId)
        {
            TypeId = typeId;
        }

        public void SetTemplate(CharacterTemplate newTemplate)
        {
            template = newTemplate;
            hitPointComponent.SetHitPoint(template.baseHealth, template.baseHealth);
            
            // 设置攻击力
            if (attackPowerComponent != null)
            {
                attackPowerComponent.SetAttackPower(template.baseAttackPower);
            }
            
            var spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = template.characterSprite;
            spriteRenderer.sortingLayerName = "Hero"; // 确保角色的SortingLayer为Hero层
            OnTemplateSet();
        }

        // 当配置被设置时调用
        protected virtual void OnTemplateSet()
        {
        }

        // 处理角色死亡
        protected virtual void OnDeath()
        {
            Debug.Log($"角色 {gameObject.name} 死亡");
            // 如果是主角死亡，弹出失败结算面板
            if (CompareTag("MainCharacter"))
            {
                var resultUI = FindObjectOfType<GameResultUIController>(true);
                if (resultUI != null)
                    resultUI.Show(false);
                else
                    Debug.LogWarning("未找到GameResultUIController，无法显示失败结算面板");
            }
        }

        // 获取当前攻击力
        public int GetAttackPower()
        {
            return attackPowerComponent?.GetAttackPower() ?? 0;
        }
    }
}