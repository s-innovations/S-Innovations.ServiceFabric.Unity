using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Practices.Unity;

namespace SInnovations.ServiceFabric.Unity
{
    internal class UnityServiceScopeFactory : IServiceScopeFactory
    {
        private readonly IUnityContainer container;

        public UnityServiceScopeFactory(IUnityContainer container)
        {
            this.container = container;
        }

        public IServiceScope CreateScope()
        {
            return new UnityServiceScope(CreateChildContainer());
        }

        private IUnityContainer CreateChildContainer()
        {
            var child = container.CreateChildContainer();
            child.AddExtension(new EnumerableExtension());
            return child;
        }
    }
}
