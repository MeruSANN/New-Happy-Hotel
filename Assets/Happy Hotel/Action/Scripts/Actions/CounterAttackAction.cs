using HappyHotel.Action.Components;
using HappyHotel.Action.Components.Parts;
using HappyHotel.Core.EntityComponent;
using HappyHotel.Core.ValueProcessing.Components;
using UnityEngine;

namespace HappyHotel.Action
{
    // 反击攻击行动，造成基础伤害，如果前一个格挡成功了则造成双倍伤害
    [AutoInitEntityComponent(typeof(AttackEntityComponent))]
    public class CounterAttackAction : ActionBase
    {
        private readonly AttackEntityComponent attackComponent;
        private BlockValueComponent blockValueComponent;

        public CounterAttackAction()
        {
            attackComponent = GetEntityComponent<AttackEntityComponent>();

            UpdateDamage();

            var attackValue = attackComponent?.GetAttackValue();
            if (attackValue != null) attackValue.onProcessorsChanged.AddListener(OnProcessorsChanged);
        }

        public int BaseDamage { get; private set; } = 1;

        // 重写GetActionValue方法，返回当前的攻击伤害
        public override int GetActionValue()
        {
            return attackComponent?.Damage ?? 0;
        }

        // 重写SetActionQueue方法，获取BlockValueComponent并监听事件
        public override void SetActionQueue(ActionQueueComponent actionQueue)
        {
            base.SetActionQueue(actionQueue);

            // 获取行动队列宿主的BlockValueComponent
            if (actionQueue?.GetHost() != null)
            {
                blockValueComponent = actionQueue.GetHost().GetBehaviorComponent<BlockValueComponent>();
                if (blockValueComponent != null)
                {
                    // 监听格挡成功状态改变事件
                    blockValueComponent.onBlockSuccessChanged += OnBlockSuccessChanged;
                    Debug.Log("反击攻击: 开始监听BlockValueComponent的格挡成功状态");
                }
            }

            UpdateDamage();
        }

        // 处理格挡成功状态改变事件
        private void OnBlockSuccessChanged(bool blockSuccessful)
        {
            UpdateDamage();
        }

        // 根据格挡成功状态更新伤害
        private void UpdateDamage()
        {
            if (attackComponent == null)
                return;

            // 检查前一个格挡是否成功
            var wasLastBlockSuccessful = blockValueComponent?.LastBlockWasSuccessful ?? false;

            // 根据格挡成功状态设置伤害
            var finalDamage = wasLastBlockSuccessful ? BaseDamage * 2 : BaseDamage;
            attackComponent.SetDamage(finalDamage);

            // 通知数值变化
            NotifyActionValueChanged(finalDamage);

            Debug.Log($"反击攻击: 更新伤害为 {finalDamage} (基础: {BaseDamage}, 格挡成功: {wasLastBlockSuccessful})");
        }

        private void OnProcessorsChanged()
        {
            NotifyActionValueChanged(GetActionValue());
        }

        ~CounterAttackAction()
        {
            if (blockValueComponent != null) blockValueComponent.onBlockSuccessChanged -= OnBlockSuccessChanged;
            var attackValue = attackComponent?.GetAttackValue();
            if (attackValue != null) attackValue.onProcessorsChanged.RemoveListener(OnProcessorsChanged);
        }

        // 设置基础伤害
        public void SetBaseDamage(int damage)
        {
            BaseDamage = Mathf.Max(0, damage);
            UpdateDamage(); // 重新计算伤害
        }

        // 获取当前计算后的伤害
        public int GetCurrentDamage()
        {
            return attackComponent?.Damage ?? BaseDamage;
        }

        // 检查是否会造成双倍伤害
        public bool WillDealDoubleDamage()
        {
            return blockValueComponent?.LastBlockWasSuccessful ?? false;
        }

        // 占位符格式化：{baseDamage} {finalDamage} {doubleDamage}
        protected override string FormatDescriptionInternal(string formattedDescription)
        {
            var final = GetCurrentDamage();
            var doubled = WillDealDoubleDamage();
            return formattedDescription
                .Replace("{baseDamage}", BaseDamage.ToString())
                .Replace("{finalDamage}", final.ToString())
                .Replace("{doubleDamage}", doubled ? "是" : "否");
        }
    }
}