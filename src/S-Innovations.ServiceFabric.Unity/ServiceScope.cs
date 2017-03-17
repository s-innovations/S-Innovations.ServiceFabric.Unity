//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Practices.Unity;

//namespace SInnovations.ServiceFabric.Unity
//{
//    public class ServiceScope : IServiceScope, IDisposable
//    {
//        private readonly IUnityContainer container;
//        private readonly IServiceProvider serviceProvider;

//        IServiceProvider IServiceScope.ServiceProvider => this.serviceProvider;

//        public ServiceScope(IUnityContainer container)
//        {
//            this.container = container.CreateChildContainer();
//            this.container.WithExtension();
//            this.serviceProvider = this.container.Resolve<IServiceProvider>();
//        }

//        void IDisposable.Dispose()
//        {
//            this.container.Dispose();
//        }
//    }
//}
