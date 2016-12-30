using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;

namespace SInnovations.ServiceFabric.Unity
{
    public class ServiceProvider : IServiceProvider
    {
        private readonly IUnityContainer container;

        public ServiceProvider(IUnityContainer container)
        {
            this.container = container;
        }

        public object GetService(Type serviceType) => this.container.Resolve(serviceType);
    }
}
