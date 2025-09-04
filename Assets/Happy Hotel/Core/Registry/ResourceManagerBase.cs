using System.Collections.Generic;

namespace HappyHotel.Core.Registry
{
    // 资源管理器基类
    public abstract class ResourceManagerBase<TObject, TTypeId, TFactory, TTemplate, TSettings>
        where TTypeId : TypeId, new()
        where TTemplate : class
        where TSettings : class
        where TFactory : IFactory<TObject, TTemplate, TSettings>
    {
        protected RegistryBase<TObject, TTypeId, TFactory, TTemplate, TSettings> registry;
        protected Dictionary<TTypeId, TTemplate> templateCache = new();

        public ResourceManagerBase<TObject, TTypeId, TFactory, TTemplate, TSettings>
            SetRegistry(RegistryBase<TObject, TTypeId, TFactory, TTemplate, TSettings> registry)
        {
            this.registry = registry;
            return this;
        }

        public virtual void Initialize()
        {
            LoadAllResources();
        }

        protected virtual void LoadAllResources()
        {
            foreach (var type in registry.GetAllTypes()) LoadTypeResources(type);
        }

        protected abstract void LoadTypeResources(TTypeId type);

        public virtual TTemplate GetTemplate(TTypeId type)
        {
            templateCache.TryGetValue(type, out var template);
            return template;
        }

        public virtual void ClearCache()
        {
            templateCache.Clear();
        }
    }
}