using HappyHotel.Card.Setting;
using HappyHotel.Core.Registry;
using HappyHotel.Equipment.Templates;

namespace HappyHotel.Card.Factories
{
    // 卡牌创建工厂接口
    public interface ICardFactory : IFactory<CardBase, CardTemplate, ICardSetting>
    {
    }
}