using HappyHotel.Action.Components.Parts;
using HappyHotel.Core.EntityComponent;

namespace HappyHotel.Action
{
    // 护甲行动，执行时为角色添加护甲值
    [AutoInitEntityComponent(typeof(ArmorEntityComponent))]
    public class ArmorAction : ActionBase
    {
        // 重写GetActionValue方法，返回当前的护甲值
        public override int GetActionValue()
        {
            var armorComponent = GetEntityComponent<ArmorEntityComponent>();
            return armorComponent?.ArmorAmount ?? 0;
        }

        // 占位符格式化：{armor}
        protected override string FormatDescriptionInternal(string formattedDescription)
        {
            var armor = GetActionValue();
            return formattedDescription
                .Replace("{armor}", armor.ToString());
        }
    }
}