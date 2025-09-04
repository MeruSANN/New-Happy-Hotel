namespace HappyHotel.Equipment
{
    // 武器描述符，存储武器类型的元数据
    public class EquipmentDescriptor
    {
        // 可以添加其他描述性信息，例如武器分类、稀有度等

        public EquipmentDescriptor(EquipmentTypeId typeId, string templatePath)
        {
            TypeId = typeId;
            TemplatePath = templatePath;
        }

        public EquipmentTypeId TypeId { get; private set; }
        public string TemplatePath { get; private set; }
    }
}