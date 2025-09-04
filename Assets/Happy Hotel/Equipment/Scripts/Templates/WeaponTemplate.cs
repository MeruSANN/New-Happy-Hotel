using UnityEngine;

namespace HappyHotel.Equipment.Templates
{
    [CreateAssetMenu(fileName = "New Weapon Template", menuName = "Happy Hotel/Item/Weapon Template")]
    public class WeaponTemplate : EquipmentTemplate
    {
        [Header("武器属性")] public int weaponDamage = 1;
    }
}