using HappyHotel.Core.Registry;

namespace HappyHotel.Reward
{
    // 奖励物品注册属性
    public class RewardItemRegistrationAttribute : RegistrationAttribute
    {
        public RewardItemRegistrationAttribute(string typeId, string templatePath = null) : base(typeId, templatePath)
        {
        }
    }
}