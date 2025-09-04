using UnityEngine;

namespace HappyHotel.Enemy.Factories
{
    [EnemyRegistration(
        "TestEnemy",
        "Templates/Test Enemy Template")]
    public class TestEnemyFactory : EnemyFactoryBase<TestEnemy>
    {
    }
}
