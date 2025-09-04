using System;

namespace HappyHotel.Core.EntityComponent
{
    // 组件执行优先级特性
    [AttributeUsage(AttributeTargets.Class)]
    public class ExecutionPriorityAttribute : Attribute
    {
        // 数值越小优先级越高，默认优先级为0
        public ExecutionPriorityAttribute(int priority = 0)
        {
            Priority = priority;
        }

        public int Priority { get; }
    }
}