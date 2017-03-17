using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Practices.Unity;

namespace SInnovations.ServiceFabric.Unity
{
    internal class UnityServiceScope : IServiceScope
    {
        private readonly IUnityContainer container;
        private readonly IServiceProvider provider;

        public UnityServiceScope(IUnityContainer container)
        {
            this.container = container;
            provider = container.Resolve<IServiceProvider>();
        }

        public IServiceProvider ServiceProvider
        {
            get { return provider; }
        }

        public void Dispose()
        {
            container.Dispose();
        }
    }
}
