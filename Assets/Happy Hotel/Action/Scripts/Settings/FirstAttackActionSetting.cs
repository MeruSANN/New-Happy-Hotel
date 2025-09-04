using UnityEngine;

namespace HappyHotel.Action.Settings
{
    [CreateAssetMenu(fileName = "First Attack Action Setting",
        menuName = "Happy Hotel/Action/First Attack Action Setting")]
    public class FirstAttackActionSetting : ScriptableObject, IActionSetting
    {
        [Header("伤害设置")] [SerializeField] private int baseDamage = 2;

        [SerializeField] private int bonusDamage = 1;

        public FirstAttackActionSetting(int baseDamage, int bonusDamage)
        {
            this.baseDamage = baseDamage;
            this.bonusDamage = bonusDamage;
        }

        public int BaseDamage => baseDamage;
        public int BonusDamage => bonusDamage;

        public void ConfigureAction(ActionBase action)
        {
            if (action is FirstAttackAction firstAttackAction)
            {
                firstAttackAction.SetBaseDamage(baseDamage);
                firstAttackAction.SetBonusDamage(bonusDamage);
            }
        }
    }
}