using HappyHotel.Card.Factories;
using HappyHotel.Card.Setting;
using HappyHotel.Core.Registry;
using HappyHotel.Core.Singleton;
using HappyHotel.Equipment.Templates;
using UnityEngine;

namespace HappyHotel.Card
{
    // 卡牌管理器，负责卡牌数据加载和创建
    [ManagedSingleton(true)]
    public class CardManager : ManagerBase<CardManager, CardBase, CardTypeId, ICardFactory, CardResourceManager,
        CardTemplate, ICardSetting>
    {
        protected override RegistryBase<CardBase, CardTypeId, ICardFactory, CardTemplate, ICardSetting> GetRegistry()
        {
            return CardRegistry.Instance;
        }

        protected override void Initialize()
        {
            base.Initialize();
            isInitialized = true;
        }

        // 提供一个方便的方法来获取 ResourceManager
        public CardResourceManager GetResourceManager()
        {
            if (!isInitialized) Debug.LogWarning("CardManager尚未初始化，ResourceManager可能不可用。");
            return resourceManager;
        }
    }
}