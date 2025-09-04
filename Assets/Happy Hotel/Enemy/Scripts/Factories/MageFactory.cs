using UnityEngine;

namespace HappyHotel.Enemy.Factories
{
    [EnemyRegistration(
        "Mage",
        "Templates/Mage Template")]
    public class MageFactory : EnemyFactoryBase<Mage>
    {
    }
}