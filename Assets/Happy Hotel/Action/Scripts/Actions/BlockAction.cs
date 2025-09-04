using HappyHotel.Action.Components.Parts;
using HappyHotel.Core.EntityComponent;

namespace HappyHotel.Action
{
    // 格挡行动，执行时为角色添加格挡值
    [AutoInitEntityComponent(typeof(BlockEntityComponent))]
    public class BlockAction : ActionBase
    {
        // 重写GetActionValue方法，返回当前的格挡值
        public override int GetActionValue()
        {
            var blockComponent = GetEntityComponent<BlockEntityComponent>();
            return blockComponent?.BlockAmount ?? 0;
        }

        // 占位符格式化：{block}
        protected override string FormatDescriptionInternal(string formattedDescription)
        {
            var block = GetActionValue();
            return formattedDescription
                .Replace("{block}", block.ToString());
        }
    }
}