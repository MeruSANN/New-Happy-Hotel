namespace HappyHotel.Shop
{
    public class ShopItemDescriptor
    {
        public ShopItemDescriptor(ShopItemTypeId typeId, string templatePath)
        {
            TypeId = typeId;
            TemplatePath = templatePath;
        }

        public ShopItemTypeId TypeId { get; }
        public string TemplatePath { get; }
    }
}