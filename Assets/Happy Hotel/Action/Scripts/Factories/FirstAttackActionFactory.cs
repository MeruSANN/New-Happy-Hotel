using HappyHotel.Action.Factories;

namespace HappyHotel.Action.Factory
{
    [ActionRegistration("FirstAttack",
        "Templates/First Attack Template")]
    public class FirstAttackActionFactory : ActionFactoryBase<FirstAttackAction>
    {
    }
}