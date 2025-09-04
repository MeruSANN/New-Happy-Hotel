using HappyHotel.Buff.Templates;
using HappyHotel.Core.Registry;

namespace HappyHotel.Buff.Factories
{
    // Buff工厂接口
    public interface IBuffFactory : IFactory<BuffBase, BuffTemplate, IBuffSetting>
    {
    }
}