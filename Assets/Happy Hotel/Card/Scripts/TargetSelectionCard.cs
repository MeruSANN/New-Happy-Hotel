using HappyHotel.Prop;
using UnityEngine;

namespace HappyHotel.Card
{
    // 目标选择卡牌基类，用于选择场地中特定目标的卡牌
    public abstract class TargetSelectionCard : CardBase
    {
        // 重写UseCard方法，统一使用接口
        public override bool UseCard()
        {
            // 对于目标选择卡牌，需要外部提供目标信息
            Debug.LogWarning("TargetSelectionCard需要目标信息，请使用UseCard(PropBase target)方法");
            return false;
        }

        // 重载UseCard方法，接受目标参数
        public virtual bool UseCard(PropBase target)
        {
            if (target == null)
            {
                Debug.LogError("目标为空，无法使用卡牌");
                return false;
            }

            // 先检查目标是否符合要求
            if (!IsValidTarget(target))
            {
                Debug.LogWarning($"目标 {target.Name} 不符合卡牌 {Name} 的要求");
                return false;
            }

            // 先调用基类的UseCard方法发送事件
            base.UseCard();

            // 然后执行目标处理逻辑
            return OnTargetSelected(target);
        }

        // 检查目标是否符合要求（由子类重写）
        public abstract bool IsValidTarget(PropBase target);

        // 当目标被选中时调用（由子类重写实现具体逻辑）
        protected abstract bool OnTargetSelected(PropBase target);
    }
}