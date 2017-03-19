using System;
using System.Fabric;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Practices.Unity;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.Practices.ObjectBuilder2;
using System.Diagnostics;
using Microsoft.Practices.Unity.ObjectBuilder;
using System.Collections.Generic;

namespace SInnovations.ServiceFabric.Unity
{

    //http://stackoverflow.com/questions/39173345/unity-with-asp-net-core-and-mvc6-core
    public sealed class CustomBuilderStrategy : BuilderStrategy
    {
        private readonly CustomBuildExtension extension;

        public CustomBuilderStrategy(CustomBuildExtension extension)
        {
            this.extension = extension;
        }

        private IUnityContainer GetUnityFromBuildContext(IBuilderContext context)
        {
            var lifetime = context.Policies.Get<ILifetimePolicy>(NamedTypeBuildKey.Make<IUnityContainer>());
            return lifetime.GetValue() as IUnityContainer;
        }



        private class DerivedTypeConstructorSelectorPolicy : IConstructorSelectorPolicy
        {

            private readonly IConstructorSelectorPolicy _originalConstructorSelectorPolicy;
            private readonly IUnityContainer _container;

            public DerivedTypeConstructorSelectorPolicy(IUnityContainer container, IConstructorSelectorPolicy originalSelectorPolicy)
            {
                this._originalConstructorSelectorPolicy = originalSelectorPolicy;
                this._container = container;
            }

            public SelectedConstructor SelectConstructor(IBuilderContext context, IPolicyList resolverPolicyDestination)
            {
                var originalConstructor = _originalConstructorSelectorPolicy.SelectConstructor(context, resolverPolicyDestination);

                if (originalConstructor.Constructor.GetParameters().All(arg => _container.CanResolve(arg.ParameterType)))
                {
                    return originalConstructor;
                }
                else
                {
                    var newSelectedConstructor = FindNewCtor(originalConstructor);
                    if (newSelectedConstructor == null)
                        return originalConstructor;

                    foreach (var newParameterResolver in
                        originalConstructor.GetParameterResolvers().Take(newSelectedConstructor.Constructor.GetParameters().Length))
                    {
                        newSelectedConstructor.AddParameterResolver(newParameterResolver);
                    }

                    return newSelectedConstructor;
                }
            }

            private SelectedConstructor FindNewCtor(SelectedConstructor originalConstructor)
            {
                var implementingType = originalConstructor.Constructor.DeclaringType;
                var constructors = implementingType.GetTypeInfo()
                  .DeclaredConstructors
                  .Where(constructor => constructor.IsPublic && constructor != originalConstructor.Constructor)
                  .ToArray();
                if (constructors.Length == 0) return null;

                if (constructors.Length == 1)
                    return new SelectedConstructor(constructors[0]);

                Array.Sort(constructors,
                   (a, b) => b.GetParameters().Length.CompareTo(a.GetParameters().Length));

                var newCtor = constructors.FirstOrDefault(c => c.GetParameters()
                    .All(arg => _container.CanResolve(arg.ParameterType)));

                if (newCtor == null)
                    return null;

                return new SelectedConstructor(newCtor);
            }


             
        }
        public override void PreBuildUp(IBuilderContext context)
        {
            if (context.Existing != null)
            {
                return;
            }



            var originalSelectorPolicy = context.Policies.Get<IConstructorSelectorPolicy>(context.BuildKey,
                out IPolicyList selectorPolicyDestination);

            selectorPolicyDestination.Set<IConstructorSelectorPolicy>(
                new DerivedTypeConstructorSelectorPolicy(
                    GetUnityFromBuildContext(context), originalSelectorPolicy),
                context.BuildKey);

        }

    }

    public sealed class CustomBuildExtension : UnityContainerExtension
    {
        //  public Func<Type, Type, String, MethodBase, IUnityContainer, Object> Constructor { get; set; }

        protected override void Initialize()
        {
            var strategy = new CustomBuilderStrategy(this);
            this.Context.Strategies.Add(strategy, UnityBuildStage.PreCreation);
        }
    }

    public static class UnityFabricExtensions
    {
        /// <summary>
        /// Add the extensions needed to make everything works. Including EnumerableExtensions.
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public static IUnityContainer WithExtension(this IUnityContainer container)
        {
            return container.AddExtension(new EnumerableExtension()).AddExtension(new CustomBuildExtension());
        }

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

        /// <summary>
        /// Configure logging in the container by registering the logger factory
        /// </summary>
        /// <param name="container"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static IUnityContainer ConfigureLogging(this IUnityContainer container, ILoggerFactory logger)
        {
            return container.RegisterInstance(logger);
        }

        /// <summary>
        /// UseConfiguration to setup root configuration.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="config"></param>
        /// <param name="contentRoot"></param>
        /// <returns></returns>
        public static IUnityContainer UseConfiguration(this IUnityContainer container, IConfiguration config, string contentRoot = null)
        {

            //var _options = new WebHostOptions(config);
            //var appEnvironment = PlatformServices.Default.Application;

            //var applicationName = _options.ApplicationName ?? appEnvironment.ApplicationName;

            //var environment = new HostingEnvironment();
            //environment.Initialize(applicationName, contentRoot ?? Directory.GetCurrentDirectory(), _options);

           // container.RegisterType<IHostingEnvironment,HostingEnvironment>();
            container.RegisterInstance(config);
            if (config is IConfigurationRoot)
            {
                container.RegisterInstance(config as IConfigurationRoot);
            }

            return container;
        }


        /// <summary>
        /// Add AspNet Core Options support to the container
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public static IUnityContainer AddOptions(this IUnityContainer container)
        {
            return container.RegisterType(typeof(IOptions<>), typeof(OptionsManager<>));
        }

        /// <summary>
        /// Build and add the configuration from configuration builder
        /// </summary>
        /// <param name="container"></param>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IUnityContainer BuildConfiguration(this IUnityContainer container, IConfigurationBuilder builder)
        {
            return container.UseConfiguration(builder.Build());
        }

        /// <summary>
        /// Build the configurationBuilder into the container
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="container"></param>
        /// <returns></returns>
        public static IConfigurationRoot Build(this IConfigurationBuilder builder, IUnityContainer container)
        {
            var a = builder.Build();
            container.RegisterInstance(a);
            container.UseConfiguration(a);
            return a;
        }

        /// <summary>
        /// Configure the T for Options<typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="container"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IUnityContainer Configure<T>(this IUnityContainer container, IConfigurationSection configuration) where T : class
        {
            container.RegisterInstance<IOptionsChangeTokenSource<T>>(typeof(T).AssemblyQualifiedName, new ConfigurationChangeTokenSource<T>(configuration));
            container.RegisterInstance<IConfigureOptions<T>>(typeof(T).AssemblyQualifiedName, new ConfigureFromConfigurationOptions<T>(configuration));
            return container;
        }
        /// <summary>
        /// Configure using a subsection name for T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="container"></param>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public static IUnityContainer Configure<T>(this IUnityContainer container, string sectionName) where T : class
        {
            return container.Configure<T>(container.Resolve<IConfigurationRoot>().GetSection(sectionName));
        }


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
            return container.WithExtension().RegisterInstance(instance).WithCoreCLR().AsFabricContainerInternal();
        }

        /// <summary>
        /// Add the rutnime from factory
        /// </summary>
        /// <param name="container"></param>
        /// <param name="factory"></param>
        /// <returns></returns>
        public static IUnityContainer AsFabricContainer(this IUnityContainer container, Func<IUnityContainer, FabricRuntime> factory)
        {
            return container.WithExtension().RegisterType<FabricRuntime>(new ContainerControlledLifetimeManager(), new InjectionFactory(factory))
                .WithCoreCLR().AsFabricContainerInternal();

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
                (context, actorType, actorFactory) =>
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
            Func<StatefulServiceContext, ActorTypeInformation, Func<ActorService, ActorId, TActor>, TActorService> ActorServiceFactory)
            where TActor : ActorBase
            where TActorService : ActorService
        {
            var logger = container.Resolve<ILoggerFactory>().CreateLogger<TActor>();
            logger.LogInformation("Registering Actor {ActorName}", typeof(TActor).Name);

            if (!container.IsRegistered<IActorDeactivationInterception>())
            {
                container.RegisterType<IActorDeactivationInterception, OnActorDeactivateInterceptor>(new HierarchicalLifetimeManager());
            }

            container.RegisterType(typeof(TActor), ActorProxyTypeFactory.CreateType<TActor>(), new HierarchicalLifetimeManager());
            ActorRuntime.RegisterActorAsync<TActor>((context, actorType) =>
            {
                try
                {
                    return ActorServiceFactory(context, actorType, (service, id) =>
                               container.CreateChildContainer()
                                   .WithExtension()
                                   .RegisterInstance(service.Context.CodePackageActivationContext, new ExternallyControlledLifetimeManager())
                                   .RegisterInstance(service, new ExternallyControlledLifetimeManager())
                                   .RegisterInstance(id, new ContainerControlledLifetimeManager()).Resolve<TActor>());
                }
                catch (Exception ex)
                {
                    logger.LogCritical(new EventId(100, "FailedToCreateActorService"), ex, "Failed to create ActorService for {ActorName}", typeof(TActor).Name);
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

            var child = container.CreateChildContainer().WithExtension();

            child.RegisterInstance<ServiceContext>(context, new ExternallyControlledLifetimeManager());
            child.RegisterInstance(context.CodePackageActivationContext, new ExternallyControlledLifetimeManager());
            child.RegisterInstance(context, new ExternallyControlledLifetimeManager());

            scopeRegistrations?.Invoke(child);

            return child;
        }

    }
}
