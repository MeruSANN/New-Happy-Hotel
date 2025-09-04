using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace HappyHotel.Core.Registry
{
    // 注册表基类
    public abstract class RegistryBase<TObject, TTypeId, TFactory, TTemplate, TSettings>
        where TTypeId : TypeId, new()
        where TTemplate : class
        where TSettings : class
        where TFactory : IFactory<TObject, TTemplate, TSettings>
    {
        protected Dictionary<TTypeId, TFactory> factories = new();
        protected Dictionary<string, TTypeId> registeredTypes = new();

        public virtual void Initialize()
        {
            // 自动扫描并注册所有带有特定注册特性的工厂类
            var factoryTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.GetCustomAttribute(GetRegistrationAttributeType()) != null);

            foreach (var factoryType in factoryTypes) RegisterFactory(factoryType);
        }

        protected abstract Type GetRegistrationAttributeType();

        protected virtual void RegisterFactory(Type factoryType)
        {
            var attr = factoryType.GetCustomAttribute(GetRegistrationAttributeType()) as RegistrationAttribute;
            if (attr == null)
            {
                Debug.LogError($"类型 {factoryType.Name} 没有正确的注册特性");
                return;
            }

            var typeId = RegisterType(attr.TypeId);

            OnRegister(attr);

            try
            {
                var factory = (TFactory)Activator.CreateInstance(factoryType);
                factories[typeId] = factory;
                Debug.Log($"已注册工厂: {attr.TypeId}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"创建工厂失败 {attr.TypeId}: {ex.Message}");
            }
        }

        public virtual TTypeId RegisterType(string typeId)
        {
            if (!registeredTypes.TryGetValue(typeId, out var registeredTypeId))
            {
                registeredTypeId = TypeId.Create<TTypeId>(typeId);
                registeredTypes[typeId] = registeredTypeId;
            }

            return registeredTypeId;
        }

        protected virtual void OnRegister(RegistrationAttribute attr)
        {
        }

        public virtual TTypeId GetType(string typeId)
        {
            return registeredTypes.GetValueOrDefault(typeId);
        }

        public virtual bool IsTypeRegistered(string typeId)
        {
            return registeredTypes.ContainsKey(typeId);
        }

        public virtual TFactory GetFactory(TTypeId type)
        {
            factories.TryGetValue(type, out var factory);
            return factory;
        }

        public virtual IEnumerable<TTypeId> GetAllTypes()
        {
            return registeredTypes.Values;
        }

        public virtual void Clear()
        {
            factories.Clear();
            registeredTypes.Clear();
        }
    }
}