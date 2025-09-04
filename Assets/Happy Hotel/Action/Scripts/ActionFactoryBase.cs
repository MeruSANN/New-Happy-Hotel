using System.Reflection;
using HappyHotel.Action.Settings;
using HappyHotel.Action.Templates;
using HappyHotel.Core.Registry;

namespace HappyHotel.Action.Factories
{
    // Action工厂基类，提供自动TypeId设置功能
    public abstract class ActionFactoryBase<TAction> : IActionFactory
        where TAction : ActionBase, new()
    {
        public ActionBase Create(ActionTemplate template, IActionSetting setting = null)
        {
            var action = new TAction();

            // 自动设置TypeId
            AutoSetTypeId(action);

            setting?.ConfigureAction(action);

            return action;
        }

        private void AutoSetTypeId(ActionBase action)
        {
            var attr = GetType().GetCustomAttribute<ActionRegistrationAttribute>();
            if (attr != null)
            {
                var typeId = TypeId.Create<ActionTypeId>(attr.TypeId);
                ((ITypeIdSettable<ActionTypeId>)action).SetTypeId(typeId);
            }
        }
    }
}