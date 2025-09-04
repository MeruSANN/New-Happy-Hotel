using System;
using System.Collections.Generic;
using HappyHotel.Core.Singleton;
using UnityEngine;

namespace HappyHotel.Core.Registry
{
    // 管理器基类
    public abstract class
        ManagerBase<TManager, TObject, TTypeId, TFactory, TResource, TTemplate, TSettings> : SingletonBase<TManager>
        where TManager : ManagerBase<TManager, TObject, TTypeId, TFactory, TResource, TTemplate, TSettings>
        where TTypeId : TypeId, new()
        where TFactory : IFactory<TObject, TTemplate, TSettings>
        where TResource : ResourceManagerBase<TObject, TTypeId, TFactory, TTemplate, TSettings>, new()
        where TTemplate : class
        where TSettings : class
    {
        protected List<TObject> activeObjects = new();

        protected bool isInitialized = false;
        protected TResource resourceManager;

        // 公开的初始化状态属性
        public bool IsInitialized => isInitialized;

        protected override void OnDestroy()
        {
            activeObjects.Clear();
            resourceManager.ClearCache();
        }

        // 对象变化事件
        public event Action<TObject> OnObjectAdded;
        public event Action<TObject> OnObjectRemoved;
        public event System.Action OnObjectsChanged;

        protected override void OnSingletonAwake()
        {
            Initialize();
        }

        protected virtual void Initialize()
        {
            var registry = GetRegistry();
            registry.Initialize();
            resourceManager = new TResource();
            resourceManager.SetRegistry(registry).Initialize();
        }

        public virtual TObject Create(TTypeId typeId, TSettings settings = null)
        {
            if (!isInitialized)
            {
                Debug.Log($"{GetType()}未初始化");
                return default;
            }

            var registry = GetRegistry();

            var factory = registry.GetFactory(typeId);
            if (factory == null)
            {
                Debug.LogError($"找不到工厂: {typeId}");
                return default;
            }

            var template = resourceManager.GetTemplate(typeId);

            var obj = factory.Create(template, settings);
            if (obj != null)
            {
                activeObjects.Add(obj);
                OnObjectAdded?.Invoke(obj);
                OnObjectsChanged?.Invoke();
            }

            return obj;
        }

        public virtual TObject Create(string typeString, TSettings settings = null)
        {
            var id = TypeId.Create<TTypeId>(typeString);
            return Create(id, settings);
        }

        public virtual void Remove(TObject obj)
        {
            if (activeObjects.Contains(obj))
            {
                activeObjects.Remove(obj);
                OnObjectRemoved?.Invoke(obj);
                OnObjectsChanged?.Invoke();
            }
        }

        public virtual List<TObject> GetAllObjects()
        {
            return new List<TObject>(activeObjects);
        }

        protected abstract RegistryBase<TObject, TTypeId, TFactory, TTemplate, TSettings> GetRegistry();
    }
}