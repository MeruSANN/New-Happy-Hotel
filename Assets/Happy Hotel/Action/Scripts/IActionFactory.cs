using HappyHotel.Action.Settings;
using HappyHotel.Action.Templates;
using HappyHotel.Core.Registry;

namespace HappyHotel.Action.Factories
{
    // Action创建工厂接口
    public interface IActionFactory : IFactory<ActionBase, ActionTemplate, IActionSetting>
    {
    }
}