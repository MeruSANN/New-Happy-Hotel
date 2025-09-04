using HappyHotel.Action.Components;
using HappyHotel.Action.Components.Parts;
using HappyHotel.Core.EntityComponent;
using HappyHotel.Core.ValueProcessing.Components;
using UnityEngine;

namespace HappyHotel.Action
{
    // 精准反击行动，基础伤害等于上一个格挡值所格挡的伤害值，如果前一个格挡成功了则造成双倍伤害
    [AutoInitEntityComponent(typeof(AttackEntityComponent))]
    public class PreciseCounterAttackAction : ActionBase
    {
        private readonly AttackEntityComponent attackComponent;
        private BlockValueComponent blockValueComponent;

        public PreciseCounterAttackAction()
        {
            attackComponent = GetEntityComponent<AttackEntityComponent>();

            UpdateDamage();
        }

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
                    Debug.Log("精准反击: 开始监听BlockValueComponent的格挡成功状态");
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

            // 获取最后阻挡的伤害值作为基础伤害
            var baseDamage = blockValueComponent?.LastBlockedDamage ?? 0;

            // 根据格挡成功状态设置伤害
            var finalDamage = wasLastBlockSuccessful ? baseDamage * 2 : baseDamage;
            attackComponent.SetDamage(finalDamage);

            // 通知数值变化
            NotifyActionValueChanged(finalDamage);

            Debug.Log($"精准反击: 更新伤害为 {finalDamage} (基础伤害={baseDamage}来自最后阻挡伤害, 格挡成功: {wasLastBlockSuccessful})");
        }

        ~PreciseCounterAttackAction()
        {
            if (blockValueComponent != null) blockValueComponent.onBlockSuccessChanged -= OnBlockSuccessChanged;
        }

        // 获取当前计算后的伤害
        public int GetCurrentDamage()
        {
            return attackComponent?.Damage ?? 0;
        }

        // 获取基础伤害（最后阻挡的伤害值）
        public int GetBaseDamage()
        {
            return blockValueComponent?.LastBlockedDamage ?? 0;
        }

        // 检查是否会造成双倍伤害
        public bool WillDealDoubleDamage()
        {
            return blockValueComponent?.LastBlockWasSuccessful ?? false;
        }

        // 占位符格式化：{baseDamage} {finalDamage} {doubleDamage}
        protected override string FormatDescriptionInternal(string formattedDescription)
        {
            var baseDmg = GetBaseDamage();
            var final = GetCurrentDamage();
            var doubled = WillDealDoubleDamage();
            return formattedDescription
                .Replace("{baseDamage}", baseDmg.ToString())
                .Replace("{finalDamage}", final.ToString())
                .Replace("{doubleDamage}", doubled ? "是" : "否");
        }
    }
}