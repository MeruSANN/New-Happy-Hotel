using UnityEngine;

namespace HappyHotel.Equipment.Templates
{
    // 主动放置卡牌模板，用于配置主动放置类型的卡牌
    [CreateAssetMenu(fileName = "New Active Placement Card Template",
        menuName = "Happy Hotel/Item/Active Placement Card Template")]
    public class ActivePlacementCardTemplate : CardTemplate
    {
        [Header("主动放置设置")] [Tooltip("是否在被触发时自动销毁")]
        public bool autoDestroyOnTrigger;

        [Header("Prop描述")] [Tooltip("Prop使用的描述，与卡牌描述区分开")] [TextArea(2, 4)]
        public string propDescription;
    }
}