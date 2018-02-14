using System;
using System.Collections.Generic;
using Happimeter.Watch.Droid.Database;
using Happimeter.Watch.Droid.ServicesBusinessLogic;

namespace Happimeter.Watch.Droid
{
    public sealed class ServiceLocator
    {
        private bool IsInitialized { get; set; }

        static readonly Lazy<ServiceLocator> instance = new Lazy<ServiceLocator>(() => new ServiceLocator());
        readonly Dictionary<Type, Lazy<object>> registeredServices = new Dictionary<Type, Lazy<object>>();

        public static ServiceLocator Instance => instance.Value;

        public void Register<TContract, TService>() where TService : new()
        {
            registeredServices[typeof(TContract)] =
                new Lazy<object>(() => Activator.CreateInstance(typeof(TService)));
        }

        public T Get<T>() where T : class
        {
            if (!IsInitialized) {
                Configure();
            }
            Lazy<object> service;
            if (registeredServices.TryGetValue(typeof(T), out service))
            {
                return (T)service.Value;
            }

            return null;
        }

        public void Configure() {
            Register<IDatabaseContext, DatabaseContext>();
            Register<IMeasurementService, MeasurementService>();
            IsInitialized = true;
        }
    }
}
