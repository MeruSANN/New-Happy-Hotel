using System.Reflection;
using HappyHotel.Card.Setting;
using HappyHotel.Core.Registry;
using HappyHotel.Equipment.Templates;

namespace HappyHotel.Card.Factories
{
    // 卡牌工厂基类，提供自动TypeId设置功能
    public abstract class CardFactoryBase<TCard> : ICardFactory
        where TCard : CardBase, new()
    {
        public CardBase Create(CardTemplate template, ICardSetting setting = null)
        {
            var card = new TCard();

            // 自动设置TypeId
            AutoSetTypeId(card);

            if (template != null) card.SetTemplate(template);

            setting?.ConfigureCard(card);

            return card;
        }

        private void AutoSetTypeId(CardBase card)
        {
            var attr = GetType().GetCustomAttribute<CardRegistrationAttribute>();
            if (attr != null)
            {
                var typeId = TypeId.Create<CardTypeId>(attr.TypeId);
                ((ITypeIdSettable<CardTypeId>)card).SetTypeId(typeId);
            }
        }
    }
}