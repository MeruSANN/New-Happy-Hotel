using UnityEngine;

namespace HappyHotel.Action
{
    // 等待行动，不执行任何操作，用于在行动循环中添加间隔
    public class WaitAction : ActionBase
    {
        public override void Execute()
        {
            // 等待行动不执行任何操作，只是消耗一个行动轮次
            Debug.Log("执行等待行动");
        }

        // 占位符格式化：无特殊占位符
        protected override string FormatDescriptionInternal(string formattedDescription)
        {
            return formattedDescription;
        }
    }
}