using HappyHotel.Action.Factories;

namespace HappyHotel.Action.Factory
{
    [ActionRegistration("FirstAttackPlus",
        "Templates/First Attack Plus Template")]
    public class FirstAttackPlusActionFactory : ActionFactoryBase<FirstAttackPlusAction>
    {
    }
}