using Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SInnovations.ServiceFabric.Unity
{
    public class OnActorDeactivateInterceptor : IActorDeactivationInterception
    {
        private readonly IUnityContainer container;
        public OnActorDeactivateInterceptor(IUnityContainer container)
        {
            this.container = container;
        }

        public void Intercept()
        {
            this.container.Dispose();
        }
    }
}
