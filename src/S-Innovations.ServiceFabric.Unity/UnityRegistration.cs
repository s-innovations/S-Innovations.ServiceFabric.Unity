using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Practices.Unity;

namespace SInnovations.ServiceFabric.Unity
{
   public static class UnityRegistration  
{
    public static void Populate(this IUnityContainer container, 
        IEnumerable<ServiceDescriptor> descriptors)
    {
        container.AddExtension(new EnumerableExtension());
            
        container.RegisterType<IServiceProvider, UnityServiceProvider>();
        container.RegisterType<IServiceScopeFactory, UnityServiceScopeFactory>();

        foreach (var descriptor in descriptors)
        {
            Register(container, descriptor);
        }
    }

    private static void Register(IUnityContainer container,
        ServiceDescriptor descriptor)
    {
        if (descriptor.ImplementationType != null)
        {
            container.RegisterType(descriptor.ServiceType, 
                descriptor.ImplementationType, 
                GetLifetimeManager(descriptor.Lifetime));
        }
        else if (descriptor.ImplementationFactory != null)
        {
            container.RegisterType(descriptor.ServiceType, 
                GetLifetimeManager(descriptor.Lifetime),
                new InjectionFactory(unity =>
                {
                    var provider = unity.Resolve<IServiceProvider>();
                    return descriptor.ImplementationFactory(provider);
                }));
        }
        else if (descriptor.ImplementationInstance != null)
        {
            container.RegisterInstance(descriptor.ServiceType, 
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
