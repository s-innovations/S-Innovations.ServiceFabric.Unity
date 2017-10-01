using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace SInnovations.Unity.AspNetCore
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
