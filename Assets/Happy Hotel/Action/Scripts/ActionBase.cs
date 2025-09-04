using System;
using System.Collections.Generic;
using HappyHotel.Action.Components;
using HappyHotel.Action.Components.Parts;
using HappyHotel.Core.Description;
using HappyHotel.Core.EntityComponent;
using HappyHotel.Core.Registry;

namespace HappyHotel.Action
{
    // 基础的Action抽象类，提供一些通用功能
    public abstract class ActionBase : EntityComponentContainer, IAction, ITypeIdSettable<ActionTypeId>,
        IFormattableDescription
    {
        // 定义一个委托和事件，用于构建行动链
        public delegate void ActionChainBuilder(Queue<IAction> chain);

        // 所属的ActionQueue
        protected ActionQueueComponent actionQueue;

        // Action的类型ID
        public ActionTypeId TypeId { get; private set; }

        /// <summary>
        ///     执行行动的核心逻辑
        ///     注意：不应该直接调用此方法来执行行动。
        ///     正确的执行方式是通过ActionConsumerComponent触发SimultaneousActionResolver，
        ///     由SimultaneousActionResolver负责按正确的顺序调用此方法。
        ///     直接调用Execute()会跳过同步执行机制和组件的优先级处理。
        /// </summary>
        public virtual void Execute()
        {
        }

        public ActionQueueComponent GetActionQueue()
        {
            return actionQueue;
        }

        public virtual void SetActionQueue(ActionQueueComponent queue)
        {
            actionQueue = queue;

            var executeEvent = new EntityComponentEvent("SetActionQueue", this, actionQueue);
            SendEvent(executeEvent);
        }

        // 获取原始描述模板（包含占位符）
        public virtual string GetDescriptionTemplate()
        {
            var template = ActionManager.Instance?.GetResourceManager()?.GetTemplate(TypeId);
            return template != null ? template.description ?? "" : "";
        }

        // 获取格式化后的描述文本
        public virtual string GetFormattedDescription()
        {
            var template = GetDescriptionTemplate();
            if (string.IsNullOrEmpty(template)) return "";
            return FormatDescriptionInternal(template);
        }

        // 实现ITypeIdSettable接口
        public void SetTypeId(ActionTypeId typeId)
        {
            TypeId = typeId;
        }

        public event ActionChainBuilder onBuildChain;

        // 这个方法将是构建行动链的入口点
        public void BuildActionChain(Queue<IAction> chain)
        {
            // 首先，将自己作为链条的第一个行动加入
            chain.Enqueue(this);

            // 触发信号，允许其他组件向链条中添加更多的行动
            onBuildChain?.Invoke(chain);
        }

        // 行动数值变化事件
        public event Action<int> onActionValueChanged;

        // 获取行动的主要数值（如攻击伤害、格挡值、护甲值等）
        // 子类应该重写此方法返回对应的数值
        public virtual int GetActionValue()
        {
            return 0; // 默认返回0，子类应该重写
        }

        // 获取行动的多个数值，用于显示双数值或多数值的行动
        // 返回数组，第一个元素是主要数值，后续元素是次要数值
        // 子类可以重写此方法返回多个数值
        public virtual int[] GetActionValues()
        {
            var primaryValue = GetActionValue();
            return primaryValue > 0 ? new[] { primaryValue } : new int[0];
        }

        // 通知行动数值发生变化
        protected void NotifyActionValueChanged(int newValue)
        {
            onActionValueChanged?.Invoke(newValue);
        }

        ~ActionBase()
        {
            ActionManager.Instance.Remove(this);
        }

        // 子类可重写以替换占位符
        protected virtual string FormatDescriptionInternal(string formattedDescription)
        {
            // 通用占位符替换（基于已附加的EntityComponent）
            // {damage} / {attackDamage}
            var attackEntityComponent = GetEntityComponent<AttackEntityComponent>();
            if (attackEntityComponent != null)
            {
                var damageStr = attackEntityComponent.Damage.ToString();
                formattedDescription = formattedDescription
                    .Replace("{damage}", damageStr)
                    .Replace("{attackDamage}", damageStr);
            }

            // {selfDamage}
            var selfDamageEntityComponent = GetEntityComponent<SelfDamageEntityComponent>();
            if (selfDamageEntityComponent != null)
                formattedDescription = formattedDescription
                    .Replace("{selfDamage}", selfDamageEntityComponent.SelfDamage.ToString());

            // {block}
            var blockEntityComponent = GetEntityComponent<BlockEntityComponent>();
            if (blockEntityComponent != null)
                formattedDescription = formattedDescription
                    .Replace("{block}", blockEntityComponent.BlockAmount.ToString());

            // {armor}
            var armorEntityComponent = GetEntityComponent<ArmorEntityComponent>();
            if (armorEntityComponent != null)
                formattedDescription = formattedDescription
                    .Replace("{armor}", armorEntityComponent.ArmorAmount.ToString());

            // {areaDamage}
            var areaAttackEntityComponent = GetEntityComponent<AreaAttackEntityComponent>();
            if (areaAttackEntityComponent != null)
                formattedDescription = formattedDescription
                    .Replace("{areaDamage}", areaAttackEntityComponent.AreaDamage.ToString());

            return formattedDescription;
        }
    }
}