using Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SInnovations.Unity.AspNetCore;
using Unity.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using Unity.Lifetime;
using Microsoft.Extensions.Options;
using Unity.Microsoft.DependencyInjection.Lifetime;
using Unity.Extension;

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

    public interface ITest1
    {

    }

    public interface ITest2
    {

    }
    public class TestOptions
    {

    }
    public class Test1 : ITest1, ITest2
    {
       
        public Test1(IOptions<TestOptions> options)
        { 

        }

    }


    [TestClass]
    public class UnitTest1
    {

        [TestMethod]
        public void Test1()
        {
            var container = new UnityContainer();
            container.RegisterType<Test1>(new ContainerControlledLifetimeManager());
            container.RegisterType<ITest1, Test1>();
            container.RegisterType<ITest2, Test1>();

            var a = container.Resolve<ITest1>();
            var b = container.Resolve<ITest1>();

            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());



        }
        [TestMethod]
        public void Test2()
        {
            var container = new UnityContainer();
            container.RegisterType<Test1>(new ContainerControlledLifetimeManager());
            container.RegisterType<ITest1, Test1>();
            container.RegisterType<ITest2, Test1>();

            var child = container.CreateChildContainer();

            var a = child.Resolve<ITest1>();
            var b = child.Resolve<ITest1>();

            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());



        }
        internal class MyOptionsExtension : UnityContainerExtension
        {
            protected override void Initialize()
            {
               
            }

            public ILifetimeContainer Lifetime => Context.Lifetime;
        }

        [TestMethod]
        public void Test3()
        {
            var container = new UnityContainer().AddExtension(new MyOptionsExtension());

            var lifetime=container.Configure< MyOptionsExtension>().Lifetime;
          
            container.RegisterType<Test1>(new HierarchicalLifetimeManager());
            container.RegisterType<ITest1, Test1>(new HierarchicalLifetimeManager());
            container.RegisterType<ITest2, Test1>(new HierarchicalLifetimeManager());


            container.RegisterType(typeof(IOptions<>), typeof(OptionsManager<>), new InjectionSingletonLifetimeManager(lifetime))
              .RegisterType(typeof(IOptionsSnapshot<>), typeof(OptionsManager<>), new HierarchicalLifetimeManager())
         .RegisterType(typeof(IOptionsMonitor<>), typeof(OptionsMonitor<>), new InjectionSingletonLifetimeManager(lifetime))
          .RegisterType(typeof(IOptionsFactory<>), typeof(OptionsFactory<>), new TransientLifetimeManager())
         .RegisterType(typeof(IOptionsMonitorCache<>), typeof(OptionsCache<>), new InjectionSingletonLifetimeManager(lifetime));

            {
                var child = container.CreateChildContainer();

                var a = child.Resolve<ITest1>();
                var b = child.Resolve<ITest2>();

                Assert.AreEqual(a.GetHashCode(), b.GetHashCode());

                child.Dispose();
            }
            {
                var child = container.CreateChildContainer();

                var a = child.Resolve<ITest1>();
                var b = child.Resolve<ITest2>();

                Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
            }
             
        }

        [TestMethod]
        public void Test4()
        {
            var serviceColection = new ServiceCollection();
            serviceColection.AddOptions();
     
            var container = new UnityContainer();
            container.RegisterType<Test1>(new HierarchicalLifetimeManager());
            container.RegisterType<ITest1, Test1>(new HierarchicalLifetimeManager());
            container.RegisterType<ITest2, Test1>(new HierarchicalLifetimeManager());

            var factory = new ServiceProviderFactory(container);

            var childcontainer = factory.CreateBuilder(serviceColection) ;

            {
                var child = childcontainer.CreateChildContainer();

                var a = child.Resolve<ITest1>();
                var b = child.Resolve<ITest2>();

                Assert.AreEqual(a.GetHashCode(), b.GetHashCode());

                child.Dispose();
            }
            {
                var child = childcontainer.CreateChildContainer();

                var a = child.Resolve<ITest1>();
                var b = child.Resolve<ITest2>();

                Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
            }

        }
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
