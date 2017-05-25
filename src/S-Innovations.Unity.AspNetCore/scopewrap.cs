using Microsoft.Extensions.DependencyInjection;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SInnovations.Unity.AspNetCore
{
    public class scopewrap : IServiceScope
    {
        private IUnityContainer container;


        public scopewrap(IUnityContainer container, IServiceScope aspNetScope)
        {
            this.container = container.CreateChildContainer();
            this.container.RegisterInstance("old", aspNetScope);
            this.container.RegisterInstance<IServiceProvider>("old", aspNetScope.ServiceProvider);


        }
        public IServiceProvider ServiceProvider => this.container.Resolve<IServiceProvider>();

        public void Dispose()
        {
            this.container.Dispose();

        }
    }
}
