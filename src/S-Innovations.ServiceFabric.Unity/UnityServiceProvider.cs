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
            return container.Resolve(serviceType);
        }
    }
}
