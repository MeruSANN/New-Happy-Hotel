using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.ValueProcessing.Processors;
using HappyHotel.Core.ValueProcessing.Modifiers;
using UnityEngine;

namespace HappyHotel.Core.ValueProcessing.Components
{
    // 角色攻击力组件，使用数值处理系统
    public class AttackPowerComponent : BehaviorComponentBase
    {
        private AttackValue attackValue;

        public int AttackPower => attackValue?.GetFinalDamage() ?? 0;

        public override void OnAttach(BehaviorComponentContainer host)
        {
            base.OnAttach(host);

            // 初始化攻击力
            attackValue = new AttackValue();
            attackValue.Initialize(host);

            Debug.Log($"{host.gameObject.name} 初始化攻击力组件");
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            attackValue?.Dispose();
        }

        // 设置基础攻击力
        public void SetAttackPower(int attackPower)
        {
            if (attackValue != null)
            {
                attackValue.SetBaseDamage(attackPower);
                Debug.Log($"{host.gameObject.name} 设置攻击力: {attackPower}");
            }
        }

        // 获取当前攻击力
        public int GetAttackPower()
        {
            return AttackPower;
        }

        // 增加攻击力
        public int AddAttackPower(int amount)
        {
            return attackValue?.AddDamage(amount, host) ?? 0;
        }

        // 减少攻击力
        public int ReduceAttackPower(int amount)
        {
            return attackValue?.ReduceDamage(amount, host) ?? 0;
        }

        // 注册攻击力处理器
        public void RegisterAttackProcessor(IValueProcessor processor)
        {
            attackValue?.RegisterProcessor(processor);
        }

        // 注销攻击力处理器
        public void UnregisterAttackProcessor(IValueProcessor processor)
        {
            attackValue?.UnregisterProcessor(processor);
        }

        // 注册攻击力修饰器
        public void RegisterAttackModifier<T>(int amount, object provider) where T : IStackableStatModifier, new()
        {
            attackValue?.RegisterStackableModifier<T>(amount, provider);
        }

        // 注册无提供者的攻击力修饰器
        public void RegisterAttackModifierWithoutSource<T>(int amount) where T : IStackableStatModifier, new()
        {
            attackValue?.RegisterStackableModifierWithoutSource<T>(amount);
        }

        // 注销攻击力修饰器
        public void UnregisterAttackModifier<T>(object provider) where T : IStackableStatModifier
        {
            attackValue?.UnregisterStackableModifier<T>(provider);
        }

        // 检查是否包含指定类型的攻击力修饰器
        public bool HasAttackModifier<T>(object provider) where T : IStackableStatModifier
        {
            return attackValue?.HasStackableModifier<T>(provider) ?? false;
        }
    }
}
