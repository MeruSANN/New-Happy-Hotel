using UnityEngine;

namespace HappyHotel.Equipment.Templates
{
    // 护甲卡牌模板，用于配置护甲卡牌
    [CreateAssetMenu(fileName = "New Armor Card Template", menuName = "Happy Hotel/Item/Armor Card Template")]
    public class ArmorCardTemplate : CardTemplate
    {
        [Header("护甲设置")] [Tooltip("护甲卡牌提供的护甲值")]
        public int armorAmount = 5;
    }
}