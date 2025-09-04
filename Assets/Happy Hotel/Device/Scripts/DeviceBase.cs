using HappyHotel.Core.BehaviorComponent;
using HappyHotel.Core.Grid.Components;
using HappyHotel.Core.Registry;
using HappyHotel.Device.Templates;
using UnityEngine;

namespace HappyHotel.Device
{
    [AutoInitComponent(typeof(GridObjectComponent), true)]
    public abstract class DeviceBase : BehaviorComponentContainer, ITypeIdSettable<DeviceTypeId>
    {
        // 装置的基本属性
        protected DeviceTemplate template;

        // 装置的类型ID
        public DeviceTypeId TypeId { get; private set; }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            DeviceManager.Instance.Remove(this);
        }

        // 实现ITypeIdSettable接口
        public void SetTypeId(DeviceTypeId typeId)
        {
            TypeId = typeId;
        }

        public void SetTemplate(DeviceTemplate newTemplate)
        {
            template = newTemplate;
            OnTemplateSet();
        }

        // 当配置被设置时调用
        protected virtual void OnTemplateSet()
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && template != null) spriteRenderer.sprite = template.deviceSprite;
        }
    }
}