using Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SInnovations.Unity.AspNetCore;
using Unity.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

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

 
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void ShouldUseDefaultValue()
        {
            //My own implementation, works
            var container = new UnityContainer().WithAspNetCoreServiceProvider();
            container.Resolve<MyTestClass>();


            //var c = new UnityContainer();
            //var spf = new ServiceProviderFactory(c);

            //var cc = spf.CreateBuilder(new ServiceCollection());
            //cc.Resolve<MyTestClass>();
        }
    }
}
