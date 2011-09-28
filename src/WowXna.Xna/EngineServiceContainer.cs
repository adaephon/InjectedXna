using System;
using System.Collections.Generic;

namespace WowXna.Xna
{
    public class EngineServiceContainer : IServiceProvider
    {
        private readonly Dictionary<Type, object> _services;

        public EngineServiceContainer()
        {
            _services = new Dictionary<Type, object>();
        }

        public void AddService(Type serviceType, object provider)
        {
            if (serviceType == null)
                throw new ArgumentNullException("serviceType");
            if (provider == null)
                throw new ArgumentNullException("provider");
            if (_services.ContainsKey(serviceType))
                throw new ArgumentException("Service is already present", "serviceType");
            if (!serviceType.IsAssignableFrom(provider.GetType()))
                throw new ArgumentException(string.Format("Provider {0} is incompatible with service type {1}",
                                                          provider.GetType().FullName, serviceType.FullName));
            _services.Add(serviceType, provider);
        }

        public void RemoveService(Type serviceType)
        {
            if (serviceType == null)
                throw new ArgumentNullException("serviceType");
            _services.Remove(serviceType);
        }
        
        public object GetService(Type serviceType)
        {
            if (serviceType == null)
                throw new ArgumentNullException("serviceType");
            object value;
            return _services.TryGetValue(serviceType, out value) ? value : null;
        }
    }
}