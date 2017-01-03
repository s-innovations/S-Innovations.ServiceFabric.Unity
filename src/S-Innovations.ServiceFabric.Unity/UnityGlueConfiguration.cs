using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Practices.Unity;

namespace SInnovations.ServiceFabric.Unity
{
    public static class TypeExtensions
    {
        public static IEnumerable<MethodInfo> GetMethods(Type type, BindingFlags flags)
        {

            return type.GetMethods(flags);
        }
        public static MethodInfo GetMethod(Type type, string name, BindingFlags flags)
        {
            return type.GetMethod(name, flags);
        }
        public static Type[] GetGenericArguments(Type type)
        {
            return type.GetGenericArguments();
        }
    }
    

    public static class UnityGlueConfiguration
    {
        public static void Register(IServiceCollection services, IUnityContainer container)
        {
          

            HashSet<Type> aggregateTypes = GetAggregateTypes(services);
            MethodInfo miRegisterInstanceOpen = RegisterInstance();
            Func<ServiceDescriptor, LifetimeManager> lifetime = GetLifetime();
            foreach (ServiceDescriptor service in services)
                RegisterType(container, lifetime, service, aggregateTypes, miRegisterInstanceOpen);
        }

        public static IUnityContainer WithCoreCLR(this IUnityContainer container)
        {
            container.RegisterType<IServiceScopeFactory, ServiceScopeFactory>();
            container.RegisterType<IServiceScope, ServiceScope>();
            container.RegisterType<IServiceProvider, ServiceProvider>();

            RegisterEnumerable(container);
            return container;
        }

        private static MethodInfo RegisterInstance()
        {
            return (TypeExtensions.GetMethods(typeof(UnityContainerExtensions), (BindingFlags)24))
                .Single(mi =>
                {
                    if (mi.Name == "RegisterInstance" && mi.IsGenericMethod)
                        return mi.GetParameters().Length == 4;
                    return false;
                });
        }

        private static HashSet<Type> GetAggregateTypes(IServiceCollection services)
        {
            return new HashSet<Type>(
                services.GroupBy(k => k.ServiceType, serviceDescriptor => serviceDescriptor)
                .Where(typeGrouping => typeGrouping.Count() > 1)
                .Select(type => type.Key));
        }

        private static void RegisterEnumerable(IUnityContainer _container)
        {
            _container.RegisterType(typeof(IEnumerable<>), new InjectionFactory((container, enumerableType, name) =>
            {
                Type type = TypeExtensions.GetGenericArguments(enumerableType).Single();
                IEnumerable<object> first = container.ResolveAll(type);
                object[] objArray;
                if (!_container.IsRegistered(type) &&
                        (TypeExtensions.GetGenericArguments(type).Length == 0 ||
                        !_container.IsRegistered(type.GetGenericTypeDefinition())))

                    objArray = new object[0];
                else
                    objArray = new object[1]
                    {
                        container.Resolve(type)
                    };

                object[] array = first.Concat(objArray).ToArray();

                return TypeExtensions
                    .GetMethod(typeof(Enumerable), "OfType", (BindingFlags)24)
                    .MakeGenericMethod(type).Invoke(null, new object[] { array });

            }) as InjectionMember);
        }

        private static Func<ServiceDescriptor, LifetimeManager> GetLifetime()
        {
            return serviceDescriptor =>
            {
                switch (serviceDescriptor.Lifetime)
                {
                    case ServiceLifetime.Singleton:
                        return (LifetimeManager)new ContainerControlledLifetimeManager();
                    case ServiceLifetime.Scoped:
                        return (LifetimeManager)new HierarchicalLifetimeManager();
                    case ServiceLifetime.Transient:
                        return (LifetimeManager)new TransientLifetimeManager();
                    default:
                        throw new NotImplementedException(string.Format("Unsupported lifetime manager type '{0}'", (object)serviceDescriptor.Lifetime));
                }
            };
        }

        private static void RegisterType(
            IUnityContainer _container,
            Func<ServiceDescriptor, LifetimeManager> fetchLifetime,
            ServiceDescriptor serviceDescriptor,
            ICollection<Type> aggregateTypes,
            MethodInfo miRegisterInstanceOpen)
        {
            LifetimeManager lifetimeManager = fetchLifetime(serviceDescriptor);
            bool isAggregateType = aggregateTypes.Contains(serviceDescriptor.ServiceType);
            if (serviceDescriptor.ImplementationType != null)
                RegisterImplementation(_container, serviceDescriptor, isAggregateType, lifetimeManager);
            else if (serviceDescriptor.ImplementationFactory != null)
            {
                RegisterFactory(_container, serviceDescriptor, isAggregateType, lifetimeManager);
            }
            else
            {
                if (serviceDescriptor.ImplementationInstance == null)
                    throw new InvalidOperationException("Unsupported registration type");
                RegisterSingleton(_container, serviceDescriptor, miRegisterInstanceOpen, isAggregateType, lifetimeManager);
            }
        }

        private static void RegisterImplementation(
            IUnityContainer _container,
            ServiceDescriptor serviceDescriptor,
            bool isAggregateType,
            LifetimeManager lifetimeManager)
        {
            if (isAggregateType)
                _container.RegisterType(serviceDescriptor.ServiceType, serviceDescriptor.ImplementationType, serviceDescriptor.ImplementationType.AssemblyQualifiedName, lifetimeManager);
            else
                _container.RegisterType(serviceDescriptor.ServiceType, serviceDescriptor.ImplementationType, lifetimeManager);
        }

        private static void RegisterFactory(
            IUnityContainer _container,
            ServiceDescriptor serviceDescriptor,
            bool isAggregateType,
            LifetimeManager lifetimeManager)
        {
            if (isAggregateType)

                _container.RegisterType(
                    serviceDescriptor.ServiceType,
                    serviceDescriptor.ImplementationType.AssemblyQualifiedName,
                    lifetimeManager,
                    new InjectionFactory(container => serviceDescriptor.ImplementationFactory(container.Resolve<IServiceProvider>())));

            else
                _container.RegisterType(serviceDescriptor.ServiceType, lifetimeManager,
                    new InjectionFactory(container => serviceDescriptor.ImplementationFactory(container.Resolve<IServiceProvider>())));
        }

        private static void RegisterSingleton(
            IUnityContainer container,
            ServiceDescriptor serviceDescriptor,
            MethodInfo miRegisterInstanceOpen,
            bool isAggregateType,
            LifetimeManager lifetimeManager)
        {
            if (isAggregateType)
            {
                Type type = typeof(string);

                if (serviceDescriptor.ImplementationType != null)
                {
                    type = serviceDescriptor.ImplementationType;
                }
                else if (serviceDescriptor.ImplementationInstance != null)
                {
                    type = serviceDescriptor.ImplementationInstance.GetType();
                }

                miRegisterInstanceOpen.MakeGenericMethod(serviceDescriptor.ServiceType)
                    .Invoke(null, new object[4] { container, type.AssemblyQualifiedName, serviceDescriptor.ImplementationInstance, lifetimeManager });
            }
            else
            {
                container.RegisterInstance(serviceDescriptor.ServiceType, serviceDescriptor.ImplementationInstance, lifetimeManager);
            }
        }
    }
}
