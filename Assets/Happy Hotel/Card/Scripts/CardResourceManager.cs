using HappyHotel.Card.Factories;
using HappyHotel.Card.Setting;
using HappyHotel.Core.Registry;
using HappyHotel.Equipment.Templates;
using UnityEngine;

namespace HappyHotel.Card
{
    public class
        CardResourceManager : ResourceManagerBase<CardBase, CardTypeId, ICardFactory, CardTemplate, ICardSetting>
    {
        protected override void LoadTypeResources(CardTypeId type)
        {
            var descriptor = (registry as CardRegistry)!.GetDescriptor(type);

            var template = Resources.Load<CardTemplate>(descriptor.TemplatePath);
            if (template != null)
                templateCache[descriptor.Type] = template;
            else
                Debug.LogWarning($"无法加载卡牌模板: {descriptor.TemplatePath}");
        }
    }
}