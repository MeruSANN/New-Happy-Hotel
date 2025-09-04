using HappyHotel.Buff.Components;
using HappyHotel.Core;
using HappyHotel.Core.Description;
using HappyHotel.Core.EntityComponent;
using HappyHotel.Core.Registry;

namespace HappyHotel.Buff
{
    public abstract class BuffBase : EntityComponentContainer, ITypeIdSettable<BuffTypeId>, IFormattableDescription
    {
        protected BuffContainer buffContainer; // 持有BuffContainer的引用

        // 构造函数，传递名称给基类
        public BuffBase() : base("Buff")
        {
        }

        public BuffTypeId TypeId { get; private set; }

        // 获取原始描述模板（包含占位符）
        public virtual string GetDescriptionTemplate()
        {
            // 通过模板系统获取描述
            var template = BuffManager.Instance?.GetResourceManager()?.GetTemplate(TypeId);
            return template?.description ?? "";
        }

        // 获取格式化后的描述文本
        public virtual string GetFormattedDescription()
        {
            var template = GetDescriptionTemplate();
            if (string.IsNullOrEmpty(template)) return "";
            return FormatDescriptionInternal(template);
        }

        // 实现ITypeIdSettable接口
        public void SetTypeId(BuffTypeId typeId)
        {
            TypeId = typeId;
        }

        // 应用Buff到目标
        public abstract void OnApply(IComponentContainer target);

        // 移除Buff从目标
        public abstract void OnRemove(IComponentContainer target);

        // 回合结束时的处理，Buff可以在此执行任何逻辑，包括移除自己
        public virtual void OnTurnEnd(int turnNumber)
        {
            // 默认行为：什么都不做
        }

        // 回合开始时的处理，供需要在回合开始触发的Buff使用
        public virtual void OnTurnStart(int turnNumber)
        {
            // 默认行为：什么都不做
        }

        // 获取Buff的显示数值，子类应该重写此方法
        public virtual int GetValue()
        {
            // 默认返回1，表示Buff存在
            return 1;
        }

        // 设置BuffContainer引用
        public void SetBuffContainer(BuffContainer container)
        {
            buffContainer = container;
        }

        // 请求移除自己
        protected void RequestRemoveSelf()
        {
            if (buffContainer != null) buffContainer.RemoveBuff(this);
        }

        // 尝试与相同类型的Buff合并
        public virtual BuffMergeResult TryMergeWith(BuffBase newBuff)
        {
            // 默认行为：不合并，允许共存
            return BuffMergeResult.CreateCoexist();
        }

        // 检查是否可以与另一个Buff合并
        public virtual bool CanMergeWith(BuffBase otherBuff)
        {
            return GetType() == otherBuff.GetType();
        }

        // 子类可重写以替换占位符
        protected virtual string FormatDescriptionInternal(string formattedDescription)
        {
            return formattedDescription;
        }
    }
}