using HappyHotel.Action.Components.Parts;
using HappyHotel.Core.EntityComponent;

namespace HappyHotel.Action
{
    // 格挡并攻击的行动，格挡优先执行，攻击后执行
    [AutoInitEntityComponent(typeof(BlockEntityComponent))]
    [AutoInitEntityComponent(typeof(AttackEntityComponent))]
    public class BlockAndAttackAction : ActionBase
    {
        private AttackEntityComponent attackComponent;
        private bool subscribed;

        public BlockAndAttackAction()
        {
            attackComponent = GetEntityComponent<AttackEntityComponent>();
            TrySubscribeProcessorsChanged();
        }

        // 重写GetActionValue方法，返回当前的攻击伤害（主要数值）
        public override int GetActionValue()
        {
            var attackComponent = GetEntityComponent<AttackEntityComponent>();
            return attackComponent?.Damage ?? 0;
        }

        // 重写GetActionValues方法，返回格挡值和攻击值
        public override int[] GetActionValues()
        {
            var blockComponent = GetEntityComponent<BlockEntityComponent>();
            attackComponent ??= GetEntityComponent<AttackEntityComponent>();

            var blockValue = blockComponent?.BlockAmount ?? 0;
            var attackValue = attackComponent?.Damage ?? 0;

            return new[] { blockValue, attackValue };
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
            NotifyActionValueChanged(GetActionValue());
        }

        // 占位符格式化：{block} {damage} {attackDamage}
        protected override string FormatDescriptionInternal(string formattedDescription)
        {
            var values = GetActionValues();
            var block = values.Length > 0 ? values[0] : 0;
            var attack = values.Length > 1 ? values[1] : GetActionValue();
            return formattedDescription
                .Replace("{block}", block.ToString())
                .Replace("{damage}", attack.ToString())
                .Replace("{attackDamage}", attack.ToString());
        }
    }
}