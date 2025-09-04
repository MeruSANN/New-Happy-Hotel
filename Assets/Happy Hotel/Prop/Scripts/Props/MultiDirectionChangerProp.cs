using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Equipment.Templates;
using UnityEngine;

namespace HappyHotel.Prop
{
    // 多重转向器道具，被触发时改变触发者的方向，支持多次触发后销毁
    public class MultiDirectionChangerProp : DirectionChangerProp
    {
        [Header("多重转向器设置")] [SerializeField] private int maxTriggerCount = 3; // 最大触发次数

        [SerializeField] private int currentTriggerCount; // 当前剩余触发次数

        // 获取模板的便捷属性
        protected MultiDirectionChangerCardTemplate MultiDirectionTemplate =>
            template as MultiDirectionChangerCardTemplate;

        protected override void Start()
        {
            base.Start();

            // 初始化触发次数
            if (MultiDirectionTemplate != null)
            {
                maxTriggerCount = MultiDirectionTemplate.maxTriggerCount;
                currentTriggerCount = maxTriggerCount;
            }
        }

        protected override void OnTemplateSet()
        {
            base.OnTemplateSet();

            // 从模板读取最大触发次数
            if (MultiDirectionTemplate != null)
            {
                maxTriggerCount = MultiDirectionTemplate.maxTriggerCount;
                currentTriggerCount = maxTriggerCount;
            }
        }

        public override void OnTriggerInternal(BehaviorComponentContainer triggerer)
        {
            // 检查是否还有剩余触发次数
            if (currentTriggerCount <= 0)
            {
                Debug.Log("多重转向器已达到最大触发次数，无法继续触发");
                return;
            }

            // 调用父类的触发逻辑（改变方向）
            base.OnTriggerInternal(triggerer);

            // 减少触发次数
            currentTriggerCount--;

            Debug.Log($"{triggerer.gameObject.name} 触发了多重转向器，剩余触发次数: {currentTriggerCount}");

            // 检查是否达到最大触发次数
            if (currentTriggerCount <= 0)
            {
                Debug.Log("多重转向器已达到最大触发次数，即将销毁");
                Destroy(gameObject);
            }
        }

        // 设置最大触发次数
        public void SetMaxTriggerCount(int count)
        {
            maxTriggerCount = Mathf.Max(1, count);
            currentTriggerCount = maxTriggerCount;
        }

        // 获取最大触发次数
        public int GetMaxTriggerCount()
        {
            return maxTriggerCount;
        }

        // 获取当前剩余触发次数
        public int GetCurrentTriggerCount()
        {
            return currentTriggerCount;
        }

        // 重置触发次数
        public void ResetTriggerCount()
        {
            currentTriggerCount = maxTriggerCount;
        }

        // 重写GetDescriptionTemplate方法，使用propDescription作为描述模板
        public override string GetDescriptionTemplate()
        {
            if (template is ActivePlacementCardTemplate activeTemplate) return activeTemplate.propDescription ?? "";
            return base.GetDescriptionTemplate();
        }

        // 重写FormatDescriptionInternal方法，替换{maxTriggerCount}、{currentTriggerCount}和{direction}占位符
        protected override string FormatDescriptionInternal(string template)
        {
            if (string.IsNullOrEmpty(template))
                return template;

            // 先调用父类的方法替换{direction}占位符
            var formatted = base.FormatDescriptionInternal(template);

            // 替换{maxTriggerCount}占位符
            formatted = formatted.Replace("{maxTriggerCount}", maxTriggerCount.ToString());

            // 替换{currentTriggerCount}占位符
            formatted = formatted.Replace("{currentTriggerCount}", currentTriggerCount.ToString());

            return formatted;
        }
    }
}