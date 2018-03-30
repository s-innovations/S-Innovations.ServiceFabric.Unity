using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Fabric;
using System.Threading;
using Unity;
using Unity.Injection;
using Unity.Lifetime;

namespace SInnovations.ServiceFabric.Unity
{


    public interface IServiceScopeInitializer
    {
        IUnityContainer InitializeScope(IUnityContainer container);
    }


    public static class UnityFabricExtensions
    {


        /// <summary>
        /// Creates a new Container from the <see cref="FabricRuntime"/>
        /// </summary>
        /// <param name="runtime"></param>
        /// <returns></returns>
        public static IUnityContainer AsFabricContainer(this FabricRuntime runtime)
        {
            return new UnityContainer().AsFabricContainer(c => runtime);
        }

        /// <summary>
        /// Add fabric runtime to the existing container
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public static IUnityContainer AsFabricContainer(this IUnityContainer container)
        {
            return container.AsFabricContainer(c => FabricRuntime.Create());
        }

       

        ///// <summary>
        ///// UseConfiguration to setup root configuration.
        ///// </summary>
        ///// <param name="container"></param>
        ///// <param name="config"></param>
        ///// <param name="contentRoot"></param>
        ///// <returns></returns>
        //public static IUnityContainer UseConfiguration(this IUnityContainer container, IConfiguration config, string contentRoot = null)
        //{

        //    //var _options = new WebHostOptions(config);
        //    //var appEnvironment = PlatformServices.Default.Application;

        //    //var applicationName = _options.ApplicationName ?? appEnvironment.ApplicationName;

        //    //var environment = new HostingEnvironment();
        //    //environment.Initialize(applicationName, contentRoot ?? Directory.GetCurrentDirectory(), _options);

        //   // container.RegisterType<IHostingEnvironment,HostingEnvironment>();
        //    container.RegisterInstance(config);
        //    if (config is IConfigurationRoot)
        //    {
        //        container.RegisterInstance(config as IConfigurationRoot);
        //    }

        //    return container;
        //}


       


        ///// <summary>
        ///// Build and add the configuration from configuration builder
        ///// </summary>
        ///// <param name="container"></param>
        ///// <param name="builder"></param>
        ///// <returns></returns>
        //public static IUnityContainer BuildConfiguration(this IUnityContainer container, IConfigurationBuilder builder)
        //{
        //    return container.UseConfiguration(builder.Build());
        //}


      

        ///// <summary>
        ///// Build the configurationBuilder into the container
        ///// </summary>
        ///// <param name="builder"></param>
        ///// <param name="container"></param>
        ///// <returns></returns>
        //public static IConfigurationRoot Build(this IConfigurationBuilder builder, IUnityContainer container)
        //{
        //    var a = builder.Build();
        //    container.RegisterInstance(a);
        //    container.UseConfiguration(a);
        //    return a;
        //}


       


        /// <summary>
        /// Setting up all the needed types to make it easier in service fabric
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>

        private static IUnityContainer AsFabricContainerInternal(this IUnityContainer container)
        {
            container.RegisterType<ICodePackageActivationContext>(new ContainerControlledLifetimeManager(), new InjectionFactory(c => FabricRuntime.GetActivationContext()));
            container.RegisterType<ConfigurationPackage>(new ContainerControlledLifetimeManager(), new InjectionFactory((c) => c.Resolve<ICodePackageActivationContext>().GetConfigurationPackageObject("config")));
            container.RegisterType<FabricClient>(new ContainerControlledLifetimeManager(), new InjectionFactory((c) => new FabricClient()));

            return container;
        }

        /// <summary>
        /// Add the runtime to the container andsetup all core logic for this container.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static IUnityContainer AsFabricContainer(this IUnityContainer container, FabricRuntime instance)
        {
            return container.RegisterInstance(instance).AsFabricContainerInternal();
        }

        /// <summary>
        /// Add the rutnime from factory
        /// </summary>
        /// <param name="container"></param>
        /// <param name="factory"></param>
        /// <returns></returns>
        public static IUnityContainer AsFabricContainer(this IUnityContainer container, Func<IUnityContainer, FabricRuntime> factory)
        {
            return container.RegisterType<FabricRuntime>(new ContainerControlledLifetimeManager(), new InjectionFactory(factory))
                .AsFabricContainerInternal();

        }

        /// <summary>
        /// Add an actor implementation to the fabric container
        /// </summary>
        /// <typeparam name="TActor"></typeparam>
        /// <param name="container"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static IUnityContainer WithActor<TActor>(this IUnityContainer container, ActorServiceSettings settings = null) where TActor : ActorBase
        {
            return container.WithActor<TActor, ActorService>(
                (servicecontainer, context, actorType, actorFactory) =>
                    new ActorService(context, actorTypeInfo: actorType, actorFactory: actorFactory, settings: settings));
        }

        /// <summary>
        /// Add an actor implementation to the fabric container
        /// </summary>
        /// <typeparam name="TActor"></typeparam>
        /// <typeparam name="TActorService"></typeparam>
        /// <param name="container"></param>
        /// <param name="ActorServiceFactory"></param>
        /// <returns></returns>
        public static IUnityContainer WithActor<TActor, TActorService>(
            this IUnityContainer container,
            Func<IUnityContainer,StatefulServiceContext, ActorTypeInformation, Func<ActorService, ActorId, TActor>, TActorService> ActorServiceFactory)
            where TActor : ActorBase
            where TActorService : ActorService
        {
            //var logger = container.Resolve<ILoggerFactory>().CreateLogger<TActor>();
            //logger.LogInformation("Registering Actor {ActorName}", typeof(TActor).Name);

          //  container = container.IntializeScope();


            if (!container.IsRegistered<IActorDeactivationInterception>())
            {
                container.RegisterType<IActorDeactivationInterception, OnActorDeactivateInterceptor>(new HierarchicalLifetimeManager());
            }

            container.RegisterType(typeof(TActor), ActorProxyTypeFactory.CreateType<TActor>(), new HierarchicalLifetimeManager());
            ActorRuntime.RegisterActorAsync<TActor>((context, actorType) =>
            {
                try
                {
                    var serviceContainer = container.IntializeScope();

                    return ActorServiceFactory(serviceContainer,  context, actorType, (service, id) =>
                               serviceContainer
                                   .IntializeScope()
                                   .RegisterInstance(service.Context.CodePackageActivationContext, new ExternallyControlledLifetimeManager())
                                   .RegisterInstance(service, new ExternallyControlledLifetimeManager())
                                   .RegisterInstance(id, new ContainerControlledLifetimeManager()).Resolve<TActor>());
                }
                catch (Exception ex)
                {
                   // logger.LogCritical(new EventId(100, "FailedToCreateActorService"), ex, "Failed to create ActorService for {ActorName}", typeof(TActor).Name);
                    throw;
                }
            }).GetAwaiter().GetResult();




            return container;
        }


        /// <summary>
        /// Add stateless service
        /// </summary>
        /// <typeparam name="TStatelessService"></typeparam>
        /// <param name="container"></param>
        /// <param name="serviceTypeName"></param>
        /// <param name="scopedRegistrations"></param>
        /// <param name="timeout"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>

        public static IUnityContainer WithStatelessService<TStatelessService>(this IUnityContainer container, string serviceTypeName, Action<IUnityContainer> scopedRegistrations = null, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken)) where TStatelessService : StatelessService
        {
            ServiceRuntime.RegisterServiceAsync(serviceTypeName, (context) => MakeServiceContainer(container, context, scopedRegistrations).Resolve<TStatelessService>(), timeout, cancellationToken).GetAwaiter().GetResult();
            return container;
        }

        /// <summary>
        /// Add statefull service
        /// </summary>
        /// <typeparam name="TStatelessService"></typeparam>
        /// <param name="container"></param>
        /// <param name="serviceTypeName"></param>
        /// <param name="scopedRegistrations"></param>
        /// <param name="timeout"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static IUnityContainer WithStatefullService<TStatelessService>(this IUnityContainer container, string serviceTypeName, Action<IUnityContainer> scopedRegistrations = null, TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken)) where TStatelessService : StatefulService
        {
            ServiceRuntime.RegisterServiceAsync(serviceTypeName, (context) => MakeServiceContainer(container, context, scopedRegistrations).Resolve<TStatelessService>(), timeout, cancellationToken).GetAwaiter().GetResult();
            return container;
        }



        private static IUnityContainer MakeServiceContainer<T>(IUnityContainer container, T context, Action<IUnityContainer> scopeRegistrations = null) where T : ServiceContext
        {

            var child = container.IntializeScope();

            child.RegisterInstance<ServiceContext>(context, new ExternallyControlledLifetimeManager());
            child.RegisterInstance(context.CodePackageActivationContext, new ExternallyControlledLifetimeManager());
            child.RegisterInstance(context, new ExternallyControlledLifetimeManager());

            scopeRegistrations?.Invoke(child);

            return child;
        }

        public static IUnityContainer IntializeScope(this IUnityContainer container)
        {
            if (container.IsRegistered<IServiceScopeInitializer>())
            {
                var child= container.Resolve<IServiceScopeInitializer>().InitializeScope(container);
               

                return child;
            }

            return container.CreateChildContainer();
        }

    }

}
