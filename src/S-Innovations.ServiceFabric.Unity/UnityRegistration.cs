using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Practices.Unity;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SInnovations.ServiceFabric.Unity
{
    public static class UnityRegistration
    {
        public static IUnityContainer Populate(this IUnityContainer container,
            IEnumerable<ServiceDescriptor> descriptors)
        {
            container.WithExtension();
            // container.AddExtension(new EnumerableExtension());
             
            container.RegisterType<IServiceProvider, UnityServiceProvider>();
            container.RegisterType<IServiceScopeFactory, UnityServiceScopeFactory>();

            

            foreach (var descriptor in descriptors.Reverse())
            {
                Register(container, descriptor);
            }
            return container;
        }

        private static void Register(IUnityContainer container,
            ServiceDescriptor descriptor)
        {
            var name = null as string;
            

            if (descriptor.ImplementationType != null)
            {
                if (container.IsRegistered(descriptor.ServiceType))
                {
                    name = descriptor.GetHashCode().ToString(); //Guid.NewGuid().ToString("N");
                }

                var constructors = descriptor.ImplementationType.GetTypeInfo()
                   .DeclaredConstructors
                   .Where(constructor => constructor.IsPublic)
                   .ToArray();

                var liftime = GetLifetimeManager(descriptor.Lifetime);
              
                    container.RegisterType(descriptor.ServiceType,
                       descriptor.ImplementationType,name,
                       liftime);
               
            }
            else if (descriptor.ImplementationFactory != null)
            {
                if (container.IsRegistered(descriptor.ServiceType))
                {
                    name = descriptor.GetHashCode().ToString();
                }

                container.RegisterType(descriptor.ServiceType,name,
                    GetLifetimeManager(descriptor.Lifetime),
                    new InjectionFactory(unity =>
                    {
                        var provider = unity.Resolve<IServiceProvider>();
                        return descriptor.ImplementationFactory(provider);
                    }));
            }
            else if (descriptor.ImplementationInstance != null)
            {
                if (container.IsRegistered(descriptor.ServiceType))
                {
                    name = descriptor.GetHashCode().ToString();
                }

                container.RegisterInstance(descriptor.ServiceType,name,
                    descriptor.ImplementationInstance,
                    GetLifetimeManager(descriptor.Lifetime));
            }
        }

        private static LifetimeManager GetLifetimeManager(ServiceLifetime lifecycle)
        {
            switch (lifecycle)
            {
                case ServiceLifetime.Singleton:
                    return new ContainerControlledLifetimeManager();
                case ServiceLifetime.Scoped:
                    return new HierarchicalLifetimeManager();
                case ServiceLifetime.Transient:
                    return new TransientLifetimeManager();
                default:
                    throw new NotSupportedException(lifecycle.ToString());
            }
        }
    }
}
