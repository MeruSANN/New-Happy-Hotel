using System.Reflection;
using HappyHotel.Core.Registry;
using HappyHotel.Equipment.Templates;
using HappyHotel.Prop.Settings;
using HappyHotel.UI.HoverDisplay.PropHover;
using UnityEngine;
using UnityEngine.UI;

namespace HappyHotel.Prop.Factories
{
    // Prop工厂基类，提供自动TypeId设置功能
    public abstract class PropFactoryBase<TProp> : IPropFactory
        where TProp : PropBase
    {
        public PropBase Create(ItemTemplate template, IPropSetting setting = null)
        {
            var propObject = new GameObject(GetPropName());
            propObject.AddComponent<SpriteRenderer>();

            var prop = propObject.AddComponent<TProp>();

            // 自动设置TypeId
            AutoSetTypeId(prop);

            if (template) prop.SetTemplate(template);
            setting?.ConfigureProp(prop);

            // 添加悬停显示支持组件
            AddHoverDisplayComponents(propObject);

            return prop;
        }

        private void AutoSetTypeId(PropBase prop)
        {
            var attr = GetType().GetCustomAttribute<PropRegistrationAttribute>();
            if (attr != null)
            {
                var typeId = TypeId.Create<PropTypeId>(attr.TypeId);
                ((ITypeIdSettable<PropTypeId>)prop).SetTypeId(typeId);
            }
        }

        // 添加悬停显示支持组件
        protected virtual void AddHoverDisplayComponents(GameObject propObject)
        {
            // 添加Canvas组件用于UI事件检测
            var canvas = propObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 1; // 确保在SpriteRenderer之上

            // 添加GraphicRaycaster组件用于UI事件检测
            propObject.AddComponent<GraphicRaycaster>();

            // 添加Image组件作为事件接收区域
            var image = propObject.AddComponent<Image>();
            image.color = new Color(0, 0, 0, 0); // 完全透明

            // 添加PropHoverReceiver组件
            propObject.AddComponent<PropHoverReceiver>();
        }

        protected virtual string GetPropName()
        {
            return typeof(TProp).Name;
        }
    }
}