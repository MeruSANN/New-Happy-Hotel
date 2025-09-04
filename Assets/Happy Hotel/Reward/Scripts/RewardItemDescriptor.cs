namespace HappyHotel.Reward
{
    // 奖励物品描述符
    public class RewardItemDescriptor
    {
        public RewardItemDescriptor(RewardItemTypeId typeId, string templatePath)
        {
            TypeId = typeId;
            TemplatePath = templatePath;
        }

        public RewardItemTypeId TypeId { get; set; }
        public string TemplatePath { get; }
    }
}