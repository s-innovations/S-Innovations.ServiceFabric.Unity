using Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SInnovations.Unity.AspNetCore;
using Unity;

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
            var container = new UnityContainer().WithExtension();
            container.Resolve<MyTestClass>();
        }
    }
}
