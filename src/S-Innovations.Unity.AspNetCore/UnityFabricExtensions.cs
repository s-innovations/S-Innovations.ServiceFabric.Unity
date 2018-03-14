using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Lifetime;
using Unity.Injection;
using Unity.Extension;

namespace SInnovations.Unity.AspNetCore
{
    public interface IConfigurationBuilderExtension
    {
        IConfigurationBuilder Extend(IConfigurationBuilder cbuilder);
    }
    public static class UnityFabricExtensions
    {
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
            container.RegisterType<IOptionsChangeTokenSource<T>>(typeof(T).AssemblyQualifiedName, new ContainerControlledLifetimeManager(),
             new InjectionFactory((c) => new ConfigurationChangeTokenSource<T>(c.Resolve<IConfigurationRoot>().GetSection(sectionName))));
            container.RegisterType<IConfigureOptions<T>>(typeof(T).AssemblyQualifiedName, new ContainerControlledLifetimeManager(),
              new InjectionFactory((c) => new ConfigureFromConfigurationOptions<T>(c.Resolve<IConfigurationRoot>().GetSection(sectionName))));
            return container;

        }

        public static IUnityContainer ConfiureBuilder<T>(this IUnityContainer container)
            where T : IConfigurationBuilderExtension
        {
            container.RegisterType<IConfigurationBuilderExtension,T>(typeof(T).AssemblyQualifiedName, new ContainerControlledLifetimeManager());


            return container;
        }

        public static IUnityContainer UseConfiguration(this IUnityContainer container, IConfigurationBuilder builder)
        {
            container.RegisterInstance(builder);
            container.RegisterType<IConfigurationRoot>(new ContainerControlledLifetimeManager(),
                new InjectionFactory((c) =>
                {


                    var extensions = c.Resolve<IEnumerable<IConfigurationBuilderExtension>>();
                    var cbuilder = c.Resolve<IConfigurationBuilder>();
                    foreach(var extension in extensions)
                    {
                        cbuilder= extension.Extend(cbuilder);
                    }
                    return cbuilder.Build();

                    }));
            container.RegisterType<IConfiguration>(new ContainerControlledLifetimeManager(), new InjectionFactory(c => c.Resolve<IConfigurationRoot>()));


            return container;

            //return container.UseConfiguration(builder.Build());
        }

#if NETSTANDARD2_0
        internal class MyOptionsExtension : UnityContainerExtension
        {
            protected override void Initialize()
            {

            }

            public ILifetimeContainer Lifetime => Context.Lifetime;
        }
#endif

        /// <summary>
        /// Add AspNet Core Options support to the container
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public static IUnityContainer AddOptions(this IUnityContainer container)
        {
#if ASPNETCORE1
            return container.RegisterType(typeof(IOptions<>), typeof(OptionsManager<>))
                            .RegisterType(typeof(IOptionsSnapshot<>), typeof(OptionsManager<>), new HierarchicalLifetimeManager())
                            .RegisterType(typeof(IOptionsMonitor<>), typeof(OptionsMonitor<>), new ContainerControlledLifetimeManager());
#endif

#if NETSTANDARD2_0
            var ext = container.AddExtension(new MyOptionsExtension()).Configure<MyOptionsExtension>();

            return container.RegisterType(typeof(IOptions<>), typeof(OptionsManager<>), new global::Unity.Microsoft.DependencyInjection.Lifetime.InjectionSingletonLifetimeManager(ext.Lifetime))
                .RegisterType(typeof(IOptionsSnapshot<>), typeof(OptionsManager<>), new HierarchicalLifetimeManager())
           .RegisterType(typeof(IOptionsMonitor<>), typeof(OptionsMonitor<>), new global::Unity.Microsoft.DependencyInjection.Lifetime.InjectionSingletonLifetimeManager(ext.Lifetime))
            .RegisterType(typeof(IOptionsFactory<>), typeof(OptionsFactory<>), new TransientLifetimeManager())
           .RegisterType(typeof(IOptionsMonitorCache<>), typeof(OptionsCache<>), new global::Unity.Microsoft.DependencyInjection.Lifetime.InjectionSingletonLifetimeManager(ext.Lifetime));
#endif

        }


        /// <summary>
        /// Add the extensions needed to make everything works. Including EnumerableExtensions.
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public static IUnityContainer WithAspNetCoreServiceProvider(this IUnityContainer container)
        {
            container.RegisterType<IServiceProvider, UnityServiceProvider>();
            container.RegisterType<IServiceScopeFactory, UnityServiceScopeFactory>();

            return container.AddExtension(new EnumerableExtension()).AddExtension(new CustomBuildExtension());
        }

    }

    

}
