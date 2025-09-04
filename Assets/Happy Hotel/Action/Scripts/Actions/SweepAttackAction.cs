using HappyHotel.Action.Components.Parts;
using HappyHotel.Core.EntityComponent;
using UnityEngine;

namespace HappyHotel.Action
{
    // 横扫攻击行动，对主目标造成伤害，然后对1格范围内的所有符合tag条件的目标造成一定比例的伤害
    [AutoInitEntityComponent(typeof(AttackEntityComponent))]
    [AutoInitEntityComponent(typeof(AreaAttackEntityComponent))]
    public class SweepAttackAction : ActionBase
    {
        private readonly AreaAttackEntityComponent areaAttackComponent;
        private readonly AttackEntityComponent attackComponent;
        private float areaDamageRatio = 0.5f; // 范围伤害比例，默认50%
        private int lastKnownDamage; // 用于检测伤害变化

        public SweepAttackAction()
        {
            attackComponent = GetEntityComponent<AttackEntityComponent>();
            areaAttackComponent = GetEntityComponent<AreaAttackEntityComponent>();

            // 设置范围攻击的范围为1格
            if (areaAttackComponent != null) areaAttackComponent.SetAttackRange(1);

            // 监听AttackValue的基础值变化事件
            if (attackComponent != null)
            {
                var attackValue = attackComponent.GetAttackValue();
                if (attackValue != null)
                {
                    attackValue.onValueChanged.AddListener(OnBaseDamageChanged);
                    attackValue.onProcessorsChanged.AddListener(OnProcessorsChanged);
                }

                // 初始设置范围伤害
                UpdateAreaDamage();
            }
        }

        ~SweepAttackAction()
        {
            // 清理事件监听
            if (attackComponent != null)
            {
                var attackValue = attackComponent.GetAttackValue();
                if (attackValue != null)
                {
                    attackValue.onValueChanged.RemoveListener(OnBaseDamageChanged);
                    attackValue.onProcessorsChanged.RemoveListener(OnProcessorsChanged);
                }
            }
        }

        // 重写Execute方法，在EntityComponent执行前更新范围伤害
        public override void Execute()
        {
            // 在行动组件执行前，确保范围伤害值是最新的
            // 这主要处理通过Processor影响伤害的情况
            CheckAndUpdateAreaDamage();
        }

        // 设置范围伤害比例
        public void SetAreaDamageRatio(float ratio)
        {
            areaDamageRatio = Mathf.Max(0f, ratio);
            UpdateAreaDamage();
        }

        // 当AttackValue基础值变化时的回调
        private void OnBaseDamageChanged(int newBaseDamage)
        {
            UpdateAreaDamage();
        }

        // 检查伤害是否变化并更新范围伤害（主要用于处理Processor影响）
        private void CheckAndUpdateAreaDamage()
        {
            var currentDamage = attackComponent?.Damage ?? 0;
            if (currentDamage != lastKnownDamage)
            {
                lastKnownDamage = currentDamage;
                UpdateAreaDamage();
            }
        }

        // 更新范围攻击伤害
        private void UpdateAreaDamage()
        {
            if (areaAttackComponent != null && attackComponent != null)
            {
                var areaDamage = Mathf.FloorToInt(attackComponent.Damage * areaDamageRatio);
                areaAttackComponent.SetAreaDamage(areaDamage);
            }
        }

        private void OnProcessorsChanged()
        {
            UpdateAreaDamage();
            NotifyActionValueChanged(GetActionValue());
        }

        // 重写GetActionValue方法，返回当前的主攻击伤害
        public override int GetActionValue()
        {
            // 每次获取时检查伤害是否变化（用于UI显示）
            CheckAndUpdateAreaDamage();
            return attackComponent?.Damage ?? 0;
        }

        // 重写GetActionValues方法，返回主攻击伤害和范围攻击伤害
        public override int[] GetActionValues()
        {
            // 每次获取时检查伤害是否变化（用于UI显示）
            CheckAndUpdateAreaDamage();

            var mainDamage = attackComponent?.Damage ?? 0;
            var areaDamage = areaAttackComponent?.AreaDamage ?? 0;

            return new[] { mainDamage, areaDamage };
        }

        public float GetAreaDamageRatio()
        {
            return areaDamageRatio;
        }

        // 占位符格式化：{damage} {areaDamage} {areaRatio}
        protected override string FormatDescriptionInternal(string formattedDescription)
        {
            var mainDamage = attackComponent?.Damage ?? 0;
            var areaDamage = areaAttackComponent?.AreaDamage ?? 0;
            return formattedDescription
                .Replace("{damage}", mainDamage.ToString())
                .Replace("{areaDamage}", areaDamage.ToString())
                .Replace("{areaRatio}", Mathf.RoundToInt(areaDamageRatio * 100f).ToString());
        }
    }
}