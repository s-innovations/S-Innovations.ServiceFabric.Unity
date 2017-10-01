using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace SInnovations.Unity.AspNetCore
{


    public class UnityWrappingServiceProvider : IServiceProvider
    {
        private IServiceProvider orignal;
        private IUnityContainer container;
        public UnityWrappingServiceProvider(IServiceProvider original, IUnityContainer container)
        {
            this.orignal = original;
            this.container = container;
        }
        public object GetService(Type serviceType)
        {

            if (serviceType == typeof(IServiceScopeFactory) || serviceType == typeof(IServiceScope))
            {
                return container.Resolve(serviceType); // TryGet(serviceType);
            }
            else if (serviceType == typeof(IUnityContainer))
            {
                return this.container;
            }
 

            return orignal.GetService(serviceType);  
        }


 
    }
}
