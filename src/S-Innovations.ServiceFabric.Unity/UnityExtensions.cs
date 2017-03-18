using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SInnovations.ServiceFabric.Unity
{
    public static class UnityExtensions
    {
        public static IUnityContainer AddSingleton<T>(this IUnityContainer container)
        {
            return container.RegisterType<T>(new ContainerControlledLifetimeManager());
        }

        public static IUnityContainer AddScoped<TFrom, TTo>(this IUnityContainer container) where TTo : TFrom
        {
            return container.RegisterType<TFrom, TTo>(new HierarchicalLifetimeManager());
        }

        public static IUnityContainer AddScoped<TTo>(this IUnityContainer container, params InjectionMember[] injectionMembers)
        {
            return container.RegisterType<TTo>(new HierarchicalLifetimeManager(), injectionMembers);
        }

    }
}
