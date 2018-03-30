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
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;

namespace SInnovations.ServiceFabric.Unity.UnitTests
{

    public class FabricContainer : UnityContainer, IServiceScopeInitializer
    {

 
        private readonly ServiceProviderFactory fac;

 

        public FabricContainer()
        {

            this.RegisterInstance<IServiceScopeInitializer>(this);

 
            fac = new ServiceProviderFactory(this);
            this.AsFabricContainer();
 

        }
        public IUnityContainer InitializeScope(IUnityContainer container)
        {
 


            var child= fac.CreateBuilder(new ServiceCollection());

            fac.CreateServiceProvider(child);

            return child;
 
        }
    }
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

    public class Test3
    {
        private readonly IServiceScopeFactory serviceScopeFactory;

        public Test3(IServiceScopeFactory serviceScopeFactory)
        {
            this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        }
    }

    public interface IMygenericTest<T>
    {

    }

    public class MyGenericTest<T> : IMygenericTest<T>
    {

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
        public void Test6()
        {
            var configuration = new ConfigurationBuilder().AddEnvironmentVariables().AddInMemoryCollection(new Dictionary<string, string> { { "KeyVault:test:foo","foo"} ,{"KeyVault:PimetrServiceBus","foo" } }).Build();

            var section = configuration.GetSection("KeyVault");
            Assert.IsNotNull(section);
            var test = section.GetSection("test");
            Assert.IsNotNull(section);
            var foo = test["foo"];
            Assert.IsNotNull(section);
            Assert.AreEqual("foo", foo);
            Assert.AreEqual("foo", configuration.GetSection("KeyVault:test:foo").Value);
            Assert.AreEqual("foo", test.GetValue<string>("foo"));

            Assert.AreEqual("foo", configuration.GetValue<string>("KeyVault:test:foo"));

            Assert.AreEqual("foo", configuration.GetSection("KeyVault:PimetrServiceBus").Value);

        }
        [TestMethod]
        public void Test5()
        {
            var container = new FabricContainer();

            var child = container.IntializeScope();
            child.RegisterType<Test3>();

            var test= child.Resolve<Test3>();
            Assert.IsNotNull(test);

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

            

            var s1 = new ServiceCollection().AddOptions();
            var s2 = new ServiceCollection().AddOptions();

            s1.AddSingleton < ITest1,Test1>();
            s2.AddSingleton<ITest1, Test1>();

            var spf1 = new ServiceProviderFactory(c);
            var spf2 = new ServiceProviderFactory(c);

            var cc1 = spf1.CreateBuilder(s1);
            var cc2 = spf2.CreateBuilder(s2);

            var sp1 = spf1.CreateServiceProvider(cc1);
            var t1 = sp1.GetRequiredService<ITest1>();

            var sp2 = spf2.CreateServiceProvider(cc2);
            var t2 = sp2.GetRequiredService<ITest1>();

            

            Assert.AreNotSame(t2, t1);

            // var sp =spf.CreateServiceProvider(cc);

            //  Assert.IsNotNull(sp.GetService<MyTestClass1>());
            //  var sp = cc.Resolve<IServiceProvider>();


            //  var ccc = cc.CreateChildContainer();
            // var spp = ccc.Resolve<IServiceProvider>();

            //cc.Resolve<MyTestClass>();
        }
        public class t : Hub
        {

        }
        [TestMethod]
        public void Test9()
        {


            var c = new UnityContainer();



            var s1 = new ServiceCollection().AddOptions();
            var s2 = new ServiceCollection().AddOptions();
          

            s1.AddSingleton<ITest1, Test1>();
            s2.AddSingleton<ITest1, Test1>();
            s1.AddSingleton(typeof(IMygenericTest<>), typeof(MyGenericTest<>));
            s2.AddSingleton(typeof(IMygenericTest<>), typeof(MyGenericTest<>));


            var sp1 = s1.BuildServiceProvider(c.CreateChildContainer());

            var t1 = sp1.GetRequiredService<ITest1>();

            var sp2 = s2.BuildServiceProvider(c.CreateChildContainer());
            var t2 = sp2.GetRequiredService<ITest1>();

            //Works for above, but not below


            Assert.AreNotSame(t2, t1);
            Assert.AreNotSame(t2.GetHashCode(), t1.GetHashCode());

            var h1 = sp1.GetRequiredService<IMygenericTest<int>>();
            var h2 = sp2.GetRequiredService<IMygenericTest<int>>();
            Assert.IsNotNull(h1, "notnull");
            Assert.AreNotSame(h1, h2, "sameobj");


        }

        [TestMethod]
        public void Test10()
        {


            var c = new UnityContainer();

            var c1 = c.CreateChildContainer();
            var c2 = c.CreateChildContainer();

            c1.RegisterType(typeof(IMygenericTest<>), typeof(MyGenericTest<>),new ContainerControlledLifetimeManager());


            var t1 = c1.Resolve<IMygenericTest<int>>();
            Assert.IsNotNull(t1);

            c2.RegisterType(typeof(IMygenericTest<>), typeof(MyGenericTest<>), new ContainerControlledLifetimeManager());


            var t2 = c2.Resolve<IMygenericTest<int>>();
            Assert.IsNotNull(t2);

            Assert.AreNotSame(t2, t1);

        }

        [TestMethod]
        public void Test8()
        {
            //My own implementation, works
            //   var container = new UnityContainer().WithAspNetCoreServiceProvider();
            //  container.Resolve<MyTestClass>();



            var c = new UnityContainer();



            var s1 = new ServiceCollection().AddOptions();
            var s2 = new ServiceCollection().AddOptions();
            s1.AddSignalRCore();
            s2.AddSignalRCore();

            s1.AddSingleton<ITest1, Test1>();
            s2.AddSingleton<ITest1, Test1>();

            var sp1 = s1.BuildServiceProvider(c);
             
             var t1 = sp1.GetRequiredService<ITest1>();

            var sp2 = s2.BuildServiceProvider(c);
            var t2 = sp2.GetRequiredService<ITest1>();

            //Works for above, but not below


            Assert.AreNotSame(t2, t1);
            Assert.AreNotSame(t2.GetHashCode(), t1.GetHashCode());

            var h1 = sp1.GetRequiredService<IHubContext<t>>();
            var h2 = sp2.GetRequiredService<IHubContext<t>>();

            Assert.AreNotSame(h1, h2,"sameobj");
            Assert.AreNotSame(h1.GetHashCode(), h2.GetHashCode(), "samehash");

            // var sp =spf.CreateServiceProvider(cc);

            //  Assert.IsNotNull(sp.GetService<MyTestClass1>());
            //  var sp = cc.Resolve<IServiceProvider>();


            //  var ccc = cc.CreateChildContainer();
            // var spp = ccc.Resolve<IServiceProvider>();

            //cc.Resolve<MyTestClass>();
        }
    }
}
