using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.ValueProcessing.Processors;
using UnityEngine;

namespace HappyHotel.Core.ValueProcessing.Components
{
    // 生命值组件 - 使用新的数值处理系统
    public class HitPointValueComponent : BehaviorComponentBase
    {
        [SerializeField] private int maxHitPoint = 100;

        public HitPointValue HitPointValue { get; private set; }

        public int MaxHitPoint => HitPointValue?.MaxValue ?? 0;
        public int CurrentHitPoint => HitPointValue?.CurrentValue ?? 0;
        public bool IsDead => HitPointValue?.IsDead ?? false;

        public override void OnAttach(BehaviorComponentContainer host)
        {
            base.OnAttach(host);

            // 初始化生命值
            HitPointValue = new HitPointValue();
            HitPointValue.Initialize(host);

            Debug.Log($"{host.gameObject.name} 初始化生命值: {maxHitPoint}");
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            HitPointValue?.Dispose();
        }

        public void SetHitPoint(int maxHitPoint, int currentHitPoint)
        {
            if (HitPointValue == null) return;

            HitPointValue.SetMaxValue(maxHitPoint);
            HitPointValue.SetCurrentValue(currentHitPoint);
        }

        // 设置最大生命值
        public void SetMaxHitPoint(int maxHitPoint)
        {
            this.maxHitPoint = maxHitPoint;
            if (HitPointValue != null) HitPointValue.SetMaxValue(maxHitPoint);
        }

        // 造成伤害
        public int TakeDamage(int damage, BehaviorComponentContainer attacker = null)
        {
            return HitPointValue?.TakeDamage(damage, attacker) ?? 0;
        }

        // 新增：带来源类型的受伤
        public int TakeDamage(int damage, DamageSourceType sourceType, BehaviorComponentContainer attacker = null)
        {
            return HitPointValue?.TakeDamage(damage, sourceType, attacker) ?? 0;
        }

        // 治疗
        public int Heal(int amount, BehaviorComponentContainer healer = null)
        {
            return HitPointValue?.Heal(amount, healer) ?? 0;
        }

        // 新增：带来源类型的治疗
        public int Heal(int amount, DamageSourceType sourceType, BehaviorComponentContainer healer = null)
        {
            return HitPointValue?.Heal(amount, sourceType, healer) ?? 0;
        }

        // 复活
        public void Revive(int hitPoints = -1)
        {
            HitPointValue?.Revive(hitPoints);
        }

        // 注册处理器
        public void RegisterProcessor(IValueProcessor processor)
        {
            HitPointValue?.RegisterProcessor(processor);
        }

        // 注销处理器
        public void UnregisterProcessor(IValueProcessor processor)
        {
            HitPointValue?.UnregisterProcessor(processor);
        }
    }
}