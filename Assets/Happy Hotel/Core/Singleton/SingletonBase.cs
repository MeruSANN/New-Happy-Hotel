using System;
using UnityEngine;

namespace HappyHotel.Core.Singleton
{
    public abstract class SingletonBase<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance { get; private set; }

        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this as T;

                // 检查是否需要在场景切换时保留
                var type = typeof(T);
                if (Attribute.GetCustomAttribute(type, typeof(ManagedSingletonAttribute)) is ManagedSingletonAttribute
                        attribute && attribute.DontDestroyOnLoad)
                    DontDestroyOnLoad(gameObject);

                OnSingletonAwake();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        protected virtual void OnSingletonAwake()
        {
        }
    }
}