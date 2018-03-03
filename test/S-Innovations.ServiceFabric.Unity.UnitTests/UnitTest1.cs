using Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SInnovations.Unity.AspNetCore;
using Unity.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SInnovations.ServiceFabric.Unity.UnitTests
{
    public interface MyTest
    {

    }
    public class MyTestClass
    {
        public MyTestClass(MyTest dependency = null)
        {
            Assert.IsNull(dependency);
        }
    }
    public class MyTestClass1
    {
         
    }



    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void ShouldUseDefaultValue()
        {
            //My own implementation, works
         //   var container = new UnityContainer().WithAspNetCoreServiceProvider();
          //  container.Resolve<MyTestClass>();


            var c = new UnityContainer();
            var spf = new ServiceProviderFactory(c);

            var cc = spf.CreateBuilder(new ServiceCollection());
           // var sp =spf.CreateServiceProvider(cc);

          //  Assert.IsNotNull(sp.GetService<MyTestClass1>());
          //  var sp = cc.Resolve<IServiceProvider>();

           
          //  var ccc = cc.CreateChildContainer();
           // var spp = ccc.Resolve<IServiceProvider>();
            
            //cc.Resolve<MyTestClass>();
        }
    }
}
