using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace SInnovations.Unity.AspNetCore
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
            return container.CreateChildContainer()
                .WithExtension();
        }
    }
}
