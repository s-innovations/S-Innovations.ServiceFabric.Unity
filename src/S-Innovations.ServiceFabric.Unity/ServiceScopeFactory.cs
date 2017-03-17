//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Practices.Unity;

//namespace SInnovations.ServiceFabric.Unity
//{
//    //public class UnityServiceScopeFactory : IServiceScopeFactory
//    //{
//    //    private readonly IUnityContainer container;
//    //    public UnityServiceScopeFactory(IUnityContainer container)
//    //    {
//    //        this.container = container;
//    //    }
//    //    public IServiceScope CreateScope()
//    //    {
//    //        return new UnityServiceScope(container.CreateChildContainer(),true);

//    //       // return new UnityIServiceScope { ServiceProvider = new UnityServiceProvider(container.CreateChildContainer()) };
//    //    }
//    //}
//    public class ServiceScopeFactory : IServiceScopeFactory
//    {
//        private readonly IUnityContainer container;

//        public ServiceScopeFactory(IUnityContainer container)
//        {
//            this.container = container;
//        }

//        IServiceScope IServiceScopeFactory.CreateScope()
//        {
//            return this.container.Resolve<IServiceScope>();
//        }
//    }
//}
