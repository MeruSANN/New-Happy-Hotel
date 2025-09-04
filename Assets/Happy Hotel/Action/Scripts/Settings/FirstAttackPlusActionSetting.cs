using UnityEngine;

namespace HappyHotel.Action.Settings
{
    [CreateAssetMenu(fileName = "First Attack Plus Action Setting",
        menuName = "Happy Hotel/Action/First Attack Plus Action Setting")]
    public class FirstAttackPlusActionSetting : ScriptableObject, IActionSetting
    {
        [Header("伤害设置")] [SerializeField] private int baseDamage = 2;

        public FirstAttackPlusActionSetting(int baseDamage)
        {
            this.baseDamage = baseDamage;
        }

        public int BaseDamage => baseDamage;

        public void ConfigureAction(ActionBase action)
        {
            if (action is FirstAttackPlusAction firstAttackPlusAction) firstAttackPlusAction.SetBaseDamage(baseDamage);
        }
    }
}