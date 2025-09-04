using UnityEngine;

namespace HappyHotel.Equipment.Templates
{
    // 多重转向器卡牌模板，支持多次触发后销毁
    [CreateAssetMenu(fileName = "New Multi Direction Changer Card Template",
        menuName = "Happy Hotel/Item/Multi Direction Changer Card Template")]
    public class MultiDirectionChangerCardTemplate : DirectionalPlacementCardTemplate
    {
        [Header("多重转向器设置")] [Tooltip("最大触发次数，达到后自动销毁")]
        public int maxTriggerCount = 3;
    }
}