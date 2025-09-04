using UnityEngine;

namespace HappyHotel.Equipment.Templates
{
    [CreateAssetMenu(fileName = "New Armor Template", menuName = "Happy Hotel/Item/Armor Shop Item Template")]
    public class ArmorTemplate : EquipmentTemplate
    {
        [Header("护甲属性")] public int armorAmount = 2;
    }
}