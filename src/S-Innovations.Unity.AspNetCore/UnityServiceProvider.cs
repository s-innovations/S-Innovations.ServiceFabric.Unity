using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace SInnovations.Unity.AspNetCore
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
            if (container.CanResolve(serviceType))
            {
                return container.Resolve(serviceType);
            }

            return null;
        }
    }
}
