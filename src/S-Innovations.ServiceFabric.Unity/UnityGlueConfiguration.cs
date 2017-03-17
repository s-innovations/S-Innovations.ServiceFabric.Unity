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
    //public static class EnumerableExtension
    //{
    //    public static MethodInfo ConcatMethod = typeof(EnumerableExtension).GetMethod("Concat", BindingFlags.Public | BindingFlags.Static);
    //    public static IEnumerable<T> Concat<T>(IEnumerable<T> first, IEnumerable<T> last)
    //    {
    //        return first.Concat(last);
    //    }
         
    //}

    public class UnityWrappingServiceProvider : IServiceProvider
    {
        private IServiceProvider orignal;
        private IUnityContainer container;
        public UnityWrappingServiceProvider(IServiceProvider original, IUnityContainer container)
        {
            this.orignal = original;
            this.container = container;
        }
        public object GetService(Type serviceType)
        {
            
            if(serviceType == typeof(IServiceScopeFactory) || serviceType == typeof(IServiceScope))
            {
                return container.Resolve(serviceType); // TryGet(serviceType);
            }else if(serviceType == typeof(IUnityContainer))
            {
                return this.container;
            }
            //else if(serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            //{
            //    return EnumerableExtension.ConcatMethod.MakeGenericMethod(serviceType.GenericTypeArguments).Invoke(null,new [] { orignal.GetService(serviceType), TryGet(serviceType) });
            //}

            //if (container.IsRegistered(serviceType) ||
            //    (serviceType.IsGenericType && serviceType.GenericTypeArguments.All(t => container.IsRegistered(t))))
            //{
            //    return TryGet(serviceType) ?? orignal.GetService(serviceType);
            //}

            return orignal.GetService(serviceType); // ?? TryGet(serviceType);
        }

        
        //private object TryGet(Type serviceType)
        //{
        //    try
        //    {
        //        return container.Resolve(serviceType);
        //    }catch(Exception ex)
        //    {
        //        return null;
        //    }
        //}
    }
    public class scopeFactory : IServiceScopeFactory
    {
        private IServiceProvider child;
        private IUnityContainer container;
        public scopeFactory(IUnityContainer container)
        {
            this.container = container;
            child = this.container.Resolve<IServiceProvider>("old");
        }
        public IServiceScope CreateScope()
        {
            
            return new scopewrap(this.container, child.CreateScope());
        }
    }

    public class scopewrap : IServiceScope
    {
        private IUnityContainer container;
       

        public scopewrap(IUnityContainer container, IServiceScope aspNetScope)
        {
            this.container = container.CreateChildContainer();
            this.container.RegisterInstance("old", aspNetScope);
            this.container.RegisterInstance<IServiceProvider>("old", aspNetScope.ServiceProvider);
           

        }
        public IServiceProvider ServiceProvider => this.container.Resolve<IServiceProvider>();

        public void Dispose()
        {
            this.container.Dispose();
            
        }
    }

    public static class UnityGlueConfiguration
    {
        
        //public static IServiceProvider GetServiceFabricServiceProvider(this IServiceCollection services)
        //{
        //    var aspNetServiceProvider = services.BuildServiceProvider();
        //    var serviceFabricContainer = aspNetServiceProvider.GetService<IUnityContainer>();
 
        //    serviceFabricContainer.RegisterInstance("old", aspNetServiceProvider);


          
        //    return serviceFabricContainer.Resolve<IServiceProvider>();
        //}
        //public static void Register(IServiceCollection services, IUnityContainer container)
        //{
          

        //    HashSet<Type> aggregateTypes = GetAggregateTypes(services);
        //    MethodInfo miRegisterInstanceOpen = RegisterInstance();
        //    Func<ServiceDescriptor, LifetimeManager> lifetime = GetLifetime();
        //    foreach (ServiceDescriptor service in services)
        //        RegisterType(container, lifetime, service, aggregateTypes, miRegisterInstanceOpen);
        //}

        public static IUnityContainer WithCoreCLR(this IUnityContainer container)
        {
           // container.RegisterType<IServiceProvider>(new HierarchicalLifetimeManager(), new InjectionFactory(c => new UnityWrappingServiceProvider(c.Resolve<IServiceProvider>("old"), c)));
           // container.RegisterType<IServiceScopeFactory, scopeFactory>(new HierarchicalLifetimeManager());

            // container.RegisterType<IServiceScopeFactory, ServiceScopeFactory>();
            // container.RegisterType<IServiceScope, ServiceScope>();
            //container.RegisterType<IServiceProvider, ServiceProvider>();

        //    RegisterEnumerable(container);
            return container;
        }

        //private static MethodInfo RegisterInstance()
        //{
        //    return (TypeExtensions.GetMethods(typeof(UnityContainerExtensions), (BindingFlags)24))
        //        .Single(mi =>
        //        {
        //            if (mi.Name == "RegisterInstance" && mi.IsGenericMethod)
        //                return mi.GetParameters().Length == 4;
        //            return false;
        //        });
        //}

        //private static HashSet<Type> GetAggregateTypes(IServiceCollection services)
        //{
        //    return new HashSet<Type>(
        //        services.GroupBy(k => k.ServiceType, serviceDescriptor => serviceDescriptor)
        //        .Where(typeGrouping => typeGrouping.Count() > 1)
        //        .Select(type => type.Key));
        //}

    //    private static void RegisterEnumerable(IUnityContainer _container)
    //    {
    //        _container.RegisterType(typeof(IEnumerable<>), new InjectionFactory((container, enumerableType, name) =>
    //        {
    //            Type type = TypeExtensions.GetGenericArguments(enumerableType).Single();
    //            IEnumerable<object> first = container.ResolveAll(type);
    //            object[] objArray;
    //            if (!_container.IsRegistered(type) &&
    //                    (TypeExtensions.GetGenericArguments(type).Length == 0 ||
    //                    !_container.IsRegistered(type.GetGenericTypeDefinition())))

    //                objArray = new object[0];
    //            else
    //                objArray = new object[1]
    //                {
    //                    container.Resolve(type)
    //                };

    //            object[] array = first.Concat(objArray).ToArray();

    //            return TypeExtensions
    //                .GetMethod(typeof(Enumerable), "OfType", (BindingFlags)24)
    //                .MakeGenericMethod(type).Invoke(null, new object[] { array });

    //        }) as InjectionMember);
    //    }

    //    private static Func<ServiceDescriptor, LifetimeManager> GetLifetime()
    //    {
    //        return serviceDescriptor =>
    //        {
    //            switch (serviceDescriptor.Lifetime)
    //            {
    //                case ServiceLifetime.Singleton:
    //                    return (LifetimeManager)new ContainerControlledLifetimeManager();
    //                case ServiceLifetime.Scoped:
    //                    return (LifetimeManager)new HierarchicalLifetimeManager();
    //                case ServiceLifetime.Transient:
    //                    return (LifetimeManager)new TransientLifetimeManager();
    //                default:
    //                    throw new NotImplementedException(string.Format("Unsupported lifetime manager type '{0}'", (object)serviceDescriptor.Lifetime));
    //            }
    //        };
    //    }

    //    private static void RegisterType(
    //        IUnityContainer _container,
    //        Func<ServiceDescriptor, LifetimeManager> fetchLifetime,
    //        ServiceDescriptor serviceDescriptor,
    //        ICollection<Type> aggregateTypes,
    //        MethodInfo miRegisterInstanceOpen)
    //    {
    //        LifetimeManager lifetimeManager = fetchLifetime(serviceDescriptor);
    //        bool isAggregateType = aggregateTypes.Contains(serviceDescriptor.ServiceType);
    //        if (serviceDescriptor.ImplementationType != null)
    //            RegisterImplementation(_container, serviceDescriptor, isAggregateType, lifetimeManager);
    //        else if (serviceDescriptor.ImplementationFactory != null)
    //        {
    //            RegisterFactory(_container, serviceDescriptor, isAggregateType, lifetimeManager);
    //        }
    //        else
    //        {
    //            if (serviceDescriptor.ImplementationInstance == null)
    //                throw new InvalidOperationException("Unsupported registration type");
    //            RegisterSingleton(_container, serviceDescriptor, miRegisterInstanceOpen, isAggregateType, lifetimeManager);
    //        }
    //    }

    //    private static void RegisterImplementation(
    //        IUnityContainer _container,
    //        ServiceDescriptor serviceDescriptor,
    //        bool isAggregateType,
    //        LifetimeManager lifetimeManager)
    //    {
    //        if (isAggregateType)
    //            _container.RegisterType(serviceDescriptor.ServiceType, serviceDescriptor.ImplementationType, serviceDescriptor.ImplementationType.AssemblyQualifiedName, lifetimeManager);
    //        else
    //            _container.RegisterType(serviceDescriptor.ServiceType, serviceDescriptor.ImplementationType, lifetimeManager);
    //    }

    //    private static void RegisterFactory(
    //        IUnityContainer _container,
    //        ServiceDescriptor serviceDescriptor,
    //        bool isAggregateType,
    //        LifetimeManager lifetimeManager)
    //    {
    //        if (isAggregateType)

    //            _container.RegisterType(
    //                serviceDescriptor.ServiceType,
    //                serviceDescriptor.ImplementationType.AssemblyQualifiedName,
    //                lifetimeManager,
    //                new InjectionFactory(container => serviceDescriptor.ImplementationFactory(container.Resolve<IServiceProvider>())));

    //        else
    //            _container.RegisterType(serviceDescriptor.ServiceType, lifetimeManager,
    //                new InjectionFactory(container => serviceDescriptor.ImplementationFactory(container.Resolve<IServiceProvider>())));
    //    }

    //    private static void RegisterSingleton(
    //        IUnityContainer container,
    //        ServiceDescriptor serviceDescriptor,
    //        MethodInfo miRegisterInstanceOpen,
    //        bool isAggregateType,
    //        LifetimeManager lifetimeManager)
    //    {
    //        if (isAggregateType)
    //        {
    //            Type type = typeof(string);

    //            if (serviceDescriptor.ImplementationType != null)
    //            {
    //                type = serviceDescriptor.ImplementationType;
    //            }
    //            else if (serviceDescriptor.ImplementationInstance != null)
    //            {
    //                type = serviceDescriptor.ImplementationInstance.GetType();
    //            }

    //            miRegisterInstanceOpen.MakeGenericMethod(serviceDescriptor.ServiceType)
    //                .Invoke(null, new object[4] { container, type.AssemblyQualifiedName, serviceDescriptor.ImplementationInstance, lifetimeManager });
    //        }
    //        else
    //        {
    //            container.RegisterInstance(serviceDescriptor.ServiceType, serviceDescriptor.ImplementationInstance, lifetimeManager);
    //        }
    //    }
    }
}
