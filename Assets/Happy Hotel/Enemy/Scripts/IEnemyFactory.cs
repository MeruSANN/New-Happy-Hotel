using HappyHotel.Core.Registry;
using HappyHotel.Enemy.Settings;
using HappyHotel.Enemy.Templates;

namespace HappyHotel.Enemy.Factories
{
    public interface IEnemyFactory : IFactory<EnemyBase, EnemyTemplate, IEnemySetting>
    {
    }
}