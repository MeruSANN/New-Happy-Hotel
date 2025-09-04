using UnityEngine;

namespace HappyHotel.Core.BehaviorComponent
{
    // 组件的抽象基类，提供基本实现
    public abstract class BehaviorComponentBase : IBehaviorComponent
    {
        // 宿主对象引用
        protected BehaviorComponentContainer host;

        // 组件是否启用
        public bool IsEnabled { get; set; } = true;

        // 当组件被添加到宿主时调用
        public virtual void OnAttach(BehaviorComponentContainer host)
        {
            this.host = host;
        }

        // 当组件从宿主移除时调用
        public virtual void OnDetach()
        {
            host = null;
        }

        // 获取宿主对象
        public BehaviorComponentContainer GetHost()
        {
            return host;
        }

        // Unity生命周期事件

        #region Unity Events

        public virtual void OnAwake()
        {
        }

        public virtual void OnStart()
        {
        }

        public virtual void OnUpdate()
        {
        }

        public virtual void OnFixedUpdate()
        {
        }

        public virtual void OnLateUpdate()
        {
        }

        public virtual void OnEnable()
        {
        }

        public virtual void OnDisable()
        {
        }

        public virtual void OnDestroy()
        {
        }

        public virtual void OnCollisionEnter(Collision collision)
        {
        }

        public virtual void OnCollisionStay(Collision collision)
        {
        }

        public virtual void OnCollisionExit(Collision collision)
        {
        }

        public virtual void OnTriggerEnter(Collider other)
        {
        }

        public virtual void OnTriggerStay(Collider other)
        {
        }

        public virtual void OnTriggerExit(Collider other)
        {
        }

        public virtual void OnCollisionEnter2D(Collision2D collision)
        {
        }

        public virtual void OnCollisionStay2D(Collision2D collision)
        {
        }

        public virtual void OnCollisionExit2D(Collision2D collision)
        {
        }

        public virtual void OnTriggerEnter2D(Collider2D other)
        {
        }

        public virtual void OnTriggerStay2D(Collider2D other)
        {
        }

        public virtual void OnTriggerExit2D(Collider2D other)
        {
        }

        public virtual void OnDrawGizmos()
        {
        }

        #endregion
    }
}