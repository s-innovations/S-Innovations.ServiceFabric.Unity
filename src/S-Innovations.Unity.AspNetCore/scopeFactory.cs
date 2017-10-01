using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace SInnovations.Unity.AspNetCore
{
    public class scopeFactory : IServiceScopeFactory
    {
        private IServiceProvider child;
        private IUnityContainer container;
        public scopeFactory(IUnityContainer container)
        {
            this.container = container;
            child = this.container.Resolve<IServiceProvider>("old");
        }
        public IServiceScope CreateScope()
        {

            return new scopewrap(this.container, child.CreateScope());
        }
    }
}
