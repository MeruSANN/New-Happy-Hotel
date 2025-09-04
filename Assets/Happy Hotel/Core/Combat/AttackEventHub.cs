using HappyHotel.Core.BehaviorComponent;
using UnityEngine;

namespace HappyHotel.Core.Combat
{
    // 攻击事件中心：统一发射与订阅攻击相关事件
    public class AttackEventHub : BehaviorComponentBase
    {
        public event System.Action<AttackEventData> onBeforeAttack;
        public event System.Action<AttackEventData> onBeforeDealDamage;
        public event System.Action<AttackEventData> onAfterDealDamage;
        public event System.Action<AttackEventData> onAfterAttack;

        public void RaiseBeforeAttack(AttackEventData data)
        {
            onBeforeAttack?.Invoke(data);
        }

        public void RaiseBeforeDealDamage(AttackEventData data)
        {
            onBeforeDealDamage?.Invoke(data);
        }

        public void RaiseAfterDealDamage(AttackEventData data)
        {
            onAfterDealDamage?.Invoke(data);
        }

        public void RaiseAfterAttack(AttackEventData data)
        {
            onAfterAttack?.Invoke(data);
        }
    }
}


