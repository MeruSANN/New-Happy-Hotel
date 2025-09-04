using HappyHotel.Action.Components.Parts;
using HappyHotel.Core.EntityComponent;

namespace HappyHotel.Action
{
    [AutoInitEntityComponent(typeof(AttackEntityComponent))]
    public class AttackAction : ActionBase
    {
        private AttackEntityComponent attackComponent;
        private bool subscribed;

        public AttackAction()
        {
            attackComponent = GetEntityComponent<AttackEntityComponent>();
            TrySubscribeProcessorsChanged();
        }

        // 重写GetActionValue方法，返回当前的攻击伤害
        public override int GetActionValue()
        {
            attackComponent ??= GetEntityComponent<AttackEntityComponent>();
            return attackComponent?.Damage ?? 0;
        }

        public void SetDamage(int damage)
        {
            attackComponent ??= GetEntityComponent<AttackEntityComponent>();
            if (attackComponent != null)
            {
                attackComponent.SetDamage(damage);
                NotifyActionValueChanged(attackComponent.Damage);
            }
        }

        private void TrySubscribeProcessorsChanged()
        {
            if (subscribed) return;
            if (attackComponent == null) return;
            var attackValue = attackComponent.GetAttackValue();
            if (attackValue == null) return;
            attackValue.onProcessorsChanged.AddListener(OnProcessorsChanged);
            subscribed = true;
        }

        private void OnProcessorsChanged()
        {
            // 处理器变化可能改变最终伤害，主动通知UI刷新
            NotifyActionValueChanged(GetActionValue());
        }

        // 占位符格式化：{damage}
        protected override string FormatDescriptionInternal(string formattedDescription)
        {
            var damage = GetActionValue();
            return formattedDescription
                .Replace("{damage}", damage.ToString());
        }
    }
}