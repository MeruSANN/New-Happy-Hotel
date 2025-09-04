using HappyHotel.Core.Registry;
using HappyHotel.Core.Singleton;
using HappyHotel.Equipment.Templates;
using HappyHotel.Shop.Factory;
using HappyHotel.Shop.Settings;
using UnityEngine;

namespace HappyHotel.Shop
{
    // 商店道具管理器，负责创建和管理商店道具数据
    [ManagedSingleton(true)]
    public class ShopItemManager : ManagerBase<ShopItemManager, ShopItemBase, ShopItemTypeId, IShopItemFactory,
        ShopItemResourceManager, ItemTemplate, IShopItemSetting>
    {
        protected override RegistryBase<ShopItemBase, ShopItemTypeId, IShopItemFactory, ItemTemplate, IShopItemSetting>
            GetRegistry()
        {
            return ShopItemRegistry.Instance;
        }

        protected override void Initialize()
        {
            base.Initialize();
            isInitialized = true;
        }

        public ShopItemBase CreateShopItem(string typeIdString, IShopItemSetting setting = null)
        {
            var typeId = TypeId.Create<ShopItemTypeId>(typeIdString);
            return CreateShopItem(typeId, setting);
        }

        // 创建商店道具
        public ShopItemBase CreateShopItem(ShopItemTypeId itemType, IShopItemSetting setting = null)
        {
            if (!isInitialized)
            {
                Debug.LogError("ShopItemManager未初始化，无法创建商店道具");
                return null;
            }

            var shopItem = Create(itemType, setting);
            if (shopItem)
            {
                shopItem.transform.parent = transform;
                Debug.Log($"已创建商店道具: {shopItem.ItemName}");
            }

            return shopItem;
        }

        // 获取模板数据
        public ItemTemplate GetTemplate(ShopItemTypeId typeId)
        {
            return resourceManager.GetTemplate(typeId);
        }
    }
}