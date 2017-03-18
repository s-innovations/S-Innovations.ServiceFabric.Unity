using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Practices.Unity;
using System.Reflection;
using Microsoft.AspNetCore.Http;

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

            foreach (var descriptor in descriptors)
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
                    name = descriptor.ImplementationType.AssemblyQualifiedName;
                }

                var constructors = descriptor.ImplementationType.GetTypeInfo()
                   .DeclaredConstructors
                   .Where(constructor => constructor.IsPublic)
                   .ToArray();

                var liftime = GetLifetimeManager(descriptor.Lifetime);
                if (constructors.Length == 0 || constructors.Length == 1)
                {
                    container.RegisterType(descriptor.ServiceType,
                       descriptor.ImplementationType,name,
                       liftime);
                }
                else
                {

                    //TODO, fiure out how to do this in unity extensions to build up and select proper constructor
                    //http://stackoverflow.com/questions/36548595/how-di-container-knows-what-constructors-need-asp-net-core


                    var ctor = constructors.OrderByDescending(b => b.GetParameters().Length).FirstOrDefault();
                    var parameters = ctor.GetParameters();

                    if (descriptor.ServiceType == typeof(IHttpContextFactory))
                    {
                        container.RegisterType(descriptor.ServiceType,
                          descriptor.ImplementationType,name,
                          liftime, new InjectionConstructor(new ResolvedParameter(parameters[0].ParameterType), new ResolvedParameter(parameters[1].ParameterType), new OptionalParameter(parameters[2].ParameterType)));
                    }
                    else
                    {
                        throw new Exception($"Multiple constructors of type {descriptor.ServiceType.Name} is not supported: {string.Join(",", parameters.Select(p=>$"{p.Name} : {p.ParameterType.Name}"))}");
                        //container.RegisterType(descriptor.ServiceType,
                        //     descriptor.ImplementationType,name,
                        //     liftime);
                    }
                    
                }


            }
            else if (descriptor.ImplementationFactory != null)
            {
                if (container.IsRegistered(descriptor.ServiceType))
                {
                    name = Guid.NewGuid().ToString("N");
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
                    name = descriptor.ImplementationInstance.GetType().AssemblyQualifiedName;
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
