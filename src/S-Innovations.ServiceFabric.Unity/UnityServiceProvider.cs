using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;

namespace SInnovations.ServiceFabric.Unity
{
    internal class UnityServiceProvider : IServiceProvider
    {
        private readonly IUnityContainer container;

        public UnityServiceProvider(IUnityContainer container)
        {
            this.container = container;
        }

        public object GetService(Type serviceType)
        {
            if (serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                return container.Resolve(serviceType);
            }

            if(serviceType.IsGenericType && container.IsRegistered(serviceType.GetGenericTypeDefinition()))
            {
                return container.Resolve(serviceType);
            }

            if (container.IsRegistered(serviceType))
            {             
                return container.Resolve(serviceType);
            }
            
            return null;
        }
    }
}
