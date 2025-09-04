using UnityEngine;

namespace HappyHotel.Equipment.Templates
{
    // 触发增幅护符模板：配置下一次道具触发的Buff增幅
    [CreateAssetMenu(fileName = "Prop Boost Charm Template", menuName = "Happy Hotel/Item/Prop Boost Charm Template")]
    public class PropBoostCharmTemplate : EquipmentTemplate
    {
        [Header("触发增幅属性")] [Tooltip("下一次道具触发攻击力提升Buff的增幅数值")]
        public int buffBonus = 2;
    }
}


