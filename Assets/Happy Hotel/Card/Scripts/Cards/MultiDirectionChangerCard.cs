using HappyHotel.Core.ValueProcessing;
using HappyHotel.Equipment.Templates;
using HappyHotel.Prop;
using HappyHotel.Prop.Settings;
using UnityEngine;

namespace HappyHotel.Card
{
    // 多重转向器卡牌，可以向指定位置放置MultiDirectionChangerProp
    public class MultiDirectionChangerCard : DirectionalPlacementCard
    {
        private readonly EquipmentValue maxTriggerCountValue = new("最大触发次数");

        public MultiDirectionChangerCard()
        {
            // 初始化数值
            maxTriggerCountValue.Initialize(this);
        }

        // 获取模板的便捷属性
        protected MultiDirectionChangerCardTemplate MultiDirectionTemplate =>
            template as MultiDirectionChangerCardTemplate;

        protected override void OnTemplateSet()
        {
            base.OnTemplateSet();

            if (template is MultiDirectionChangerCardTemplate multiDirectionTemplate)
                maxTriggerCountValue.SetBaseValue(multiDirectionTemplate.maxTriggerCount);
        }

        public override PropBase PlaceProp(Vector2Int position, IPropSetting setting = null)
        {
            if (template == null)
            {
                Debug.LogError("MultiDirectionChangerCard的模板为空");
                return null;
            }

            // 确定要创建的Prop类型ID
            var propTypeIdString = TypeId.Id;

            // 创建对应的PropTypeId
            var propTypeId = Core.Registry.TypeId.Create<PropTypeId>(propTypeIdString);

            // 使用PropController放置道具
            var propController = PropController.Instance;
            if (propController == null)
            {
                Debug.LogError("PropController未初始化");
                return null;
            }

            // 使用传入的设置放置道具
            var prop = propController.PlaceProp(position, propTypeId, setting) as MultiDirectionChangerProp;

            if (prop != null)
            {
                // 从私有字段读取最大触发次数并设置到Prop
                prop.SetMaxTriggerCount(maxTriggerCountValue);

                Debug.Log($"成功放置多重转向器道具到位置: {position}，最大触发次数: {maxTriggerCountValue}");
            }
            else
            {
                Debug.LogError($"无法放置多重转向器道具到位置: {position}");
            }

            return prop;
        }

        // 获取最大触发次数
        public int GetMaxTriggerCount()
        {
            return maxTriggerCountValue;
        }

        // 设置最大触发次数
        public void SetMaxTriggerCount(int count)
        {
            maxTriggerCountValue.SetBaseValue(count);
        }

        // 重写GetDescriptionTemplate方法，使用description作为描述模板
        public override string GetDescriptionTemplate()
        {
            return template?.description ?? "";
        }

        // 重写FormatDescriptionInternal方法，替换{maxTriggerCount}占位符
        protected override string FormatDescriptionInternal(string template)
        {
            if (string.IsNullOrEmpty(template))
                return template;

            // 获取当前最大触发次数
            var maxTriggerCount = GetMaxTriggerCount();

            // 替换{maxTriggerCount}占位符
            return template.Replace("{maxTriggerCount}", maxTriggerCount.ToString());
        }
    }
}