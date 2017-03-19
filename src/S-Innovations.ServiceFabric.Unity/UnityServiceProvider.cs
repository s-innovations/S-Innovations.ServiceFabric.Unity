using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;

namespace SInnovations.ServiceFabric.Unity
{
    public static class UnityTypeExtension
    {
        public static bool CanResolve(this IUnityContainer container, Type type)
        {
            if (type.IsClass)
                return true;

            if (type.IsGenericType)
            {
                var gerericType = type.GetGenericTypeDefinition();
                if (gerericType == typeof(IEnumerable<>) ||
                    gerericType.IsClass ||
                    container.IsRegistered(gerericType))
                {
                    return true;
                }
            }

            return container.IsRegistered(type);
        }
    }
    internal class UnityServiceProvider : IServiceProvider
    {
        private readonly IUnityContainer container;

        public UnityServiceProvider(IUnityContainer container)
        {
            this.container = container;
        }

        public object GetService(Type serviceType)
        {
            if (container.CanResolve(serviceType))
            {
                return container.Resolve(serviceType);
            }
            
            return null;
        }
    }
}
