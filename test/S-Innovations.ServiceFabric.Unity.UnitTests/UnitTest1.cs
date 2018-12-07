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
using IdentityServer4.Configuration;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Reflection.Emit;
using System.Linq;
using Microsoft.AspNetCore.Hosting.Internal;
using System.Net.Http;
using System.Threading;

namespace SInnovations.ServiceFabric.Unity.UnitTests
{

    //    public class FabricContainer : UnityContainer, IServiceScopeInitializer
    //    {


    //        private readonly ServiceProviderFactory fac;



    //        public FabricContainer()
    //        {

    //            this.RegisterInstance<IServiceScopeInitializer>(this);


    //            fac = new ServiceProviderFactory(this);
    //            this.AsFabricContainer();


    //        }
    //        public IUnityContainer InitializeScope(IUnityContainer container)
    //        {



    //            var child= fac.CreateBuilder(new ServiceCollection());

    //            fac.CreateServiceProvider(child);

    //            return child;

    //        }
    //    }
    //    public interface MyTest
    //    {

    //    }
    //    public class MyTestClass
    //    {
    //        public MyTestClass(MyTest dependency = null)
    //        {
    //            Assert.IsNull(dependency);
    //        }
    //    }
    //    public class MyTestClass1
    //    {

    //    }

    //    public interface ITest1
    //    {

    //    }

    //    public interface ITest2
    //    {

    //    }
    //    public class TestOptions
    //    {

    //    }
    //    public class Test1 : ITest1, ITest2
    //    {

    //        public Test1(IOptions<TestOptions> options)
    //        { 

    //        }

    //    }

    ////    public class ReplaceTest
    ////    {
    ////        private Func<Type, bool> _isTypeExplicitlyRegistered;
    ////        private Func<Type, string, bool> _isExplicitlyRegistered;

    ////        internal Type GetFinalType(Type argType)
    ////        {

    ////            Type next;
    ////            for (var type = argType; null != type; type = next)
    ////            {
    ////                var info = type.GetTypeInfo();

    ////                if (type.IsArray)
    ////                {
    ////                    next = type.GetElementType();
    ////                    if (_isTypeExplicitlyRegistered(next)) return next;
    ////                }
    ////                else if (info.IsGenericType) //this should be IsEnumerable || IsLazy only ? not all other generics
    ////                {
    ////                    var definition = info.GetGenericTypeDefinition();
    ////                    if (definition == typeof(Lazy<>) || definition == typeof(IEnumerable<>))
    ////                    {

    ////                        if (_isTypeExplicitlyRegistered(type)) return type;


    ////                        if (_isTypeExplicitlyRegistered(definition)) return definition;

    ////                        next = info.GenericTypeArguments[0];
    ////                        if (_isTypeExplicitlyRegistered(next)) return next;
    ////                    }
    ////                    else
    ////                    {
    ////                        return type;
    ////                    }
    ////                }
    ////                else
    ////                {
    ////                    return type;
    ////                }
    ////            }

    ////            return argType;
    ////        }

    ////        public static IntPtr GetMethodAddress(MethodBase method)
    ////        {
    ////            if ((method is DynamicMethod))
    ////            {
    ////                unsafe
    ////                {
    ////                    byte* ptr = (byte*)GetDynamicMethodRuntimeHandle(method).ToPointer();
    ////                    if (IntPtr.Size == 8)
    ////                    {
    ////                        ulong* address = (ulong*)ptr;
    ////                        address += 6;
    ////                        return new IntPtr(address);
    ////                    }
    ////                    else
    ////                    {
    ////                        uint* address = (uint*)ptr;
    ////                        address += 6;
    ////                        return new IntPtr(address);
    ////                    }
    ////                }
    ////            }

    ////            RuntimeHelpers.PrepareMethod(method.MethodHandle);

    ////            unsafe
    ////            {
    ////                // Some dwords in the met
    ////                int skip = 10;

    ////                // Read the method index.
    ////                UInt64* location = (UInt64*)(method.MethodHandle.Value.ToPointer());
    ////                int index = (int)(((*location) >> 32) & 0xFF);

    ////                if (IntPtr.Size == 8)
    ////                {
    ////                    // Get the method table
    ////                    ulong* classStart = (ulong*)method.DeclaringType.TypeHandle.Value.ToPointer();
    ////                    ulong* address = classStart + index + skip;
    ////                    return new IntPtr(address);
    ////                }
    ////                else
    ////                {
    ////                    // Get the method table
    ////                    uint* classStart = (uint*)method.DeclaringType.TypeHandle.Value.ToPointer();
    ////                    uint* address = classStart + index + skip;
    ////                    return new IntPtr(address);
    ////                }
    ////            }
    ////        }

    ////        private static IntPtr GetDynamicMethodRuntimeHandle(MethodBase method)
    ////        {
    ////            if (method is DynamicMethod)
    ////            {
    ////                FieldInfo fieldInfo = typeof(DynamicMethod).GetField("m_method",
    ////                                      BindingFlags.NonPublic | BindingFlags.Instance);
    ////                return ((RuntimeMethodHandle)fieldInfo.GetValue(method)).Value;
    ////            }
    ////            return method.MethodHandle.Value;
    ////        }

    ////        public static void ReplaceMethod(IntPtr srcAdr, MethodBase dest)
    ////        {
    ////            IntPtr destAdr = GetMethodAddress(dest);
    ////            unsafe
    ////            {
    ////                if (IntPtr.Size == 8)
    ////                {
    ////                    ulong* d = (ulong*)destAdr.ToPointer();
    ////                    *d = *((ulong*)srcAdr.ToPointer());
    ////                }
    ////                else
    ////                {
    ////                    uint* d = (uint*)destAdr.ToPointer();
    ////                    *d = *((uint*)srcAdr.ToPointer());
    ////                }
    ////            }
    ////        }
    ////        public static void ReplaceMethod(MethodBase source, MethodBase dest)
    ////        {

    ////            ReplaceMethod(GetMethodAddress(source), dest);
    ////        }

    ////        public static void Install()
    ////        {
    ////            var methodToReplace = typeof(UnityContainer).GetMethod("GetFinalType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
    ////            var methodToInject = typeof(ReplaceTest).GetMethod("GetFinalType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);



    ////            RuntimeHelpers.PrepareMethod(methodToReplace.MethodHandle);
    ////            RuntimeHelpers.PrepareMethod(methodToInject.MethodHandle);

    ////            unsafe
    ////            {
    ////                if (IntPtr.Size == 4)
    ////                {
    ////                    int* inj = (int*)methodToInject.MethodHandle.Value.ToPointer() + 2;
    ////                    int* tar = (int*)methodToReplace.MethodHandle.Value.ToPointer() + 2;
    ////#if DEBUG
    ////                    Console.WriteLine("\nVersion x86 Debug\n");

    ////                    byte* injInst = (byte*)*inj;
    ////                    byte* tarInst = (byte*)*tar;

    ////                    int* injSrc = (int*)(injInst + 1);
    ////                    int* tarSrc = (int*)(tarInst + 1);

    ////                    *tarSrc = (((int)injInst + 5) + *injSrc) - ((int)tarInst + 5);
    ////#else
    ////                    Console.WriteLine("\nVersion x86 Release\n");
    ////                    *tar = *inj;
    ////#endif
    ////                }
    ////                else
    ////                {

    ////                    long* inj = (long*)methodToInject.MethodHandle.Value.ToPointer() + 1;
    ////                    long* tar = (long*)methodToReplace.MethodHandle.Value.ToPointer() + 1;
    ////#if DEBUG
    ////                    Console.WriteLine("\nVersion x64 Debug\n");
    ////                    byte* injInst = (byte*)*inj;
    ////                    byte* tarInst = (byte*)*tar;


    ////                    int* injSrc = (int*)(injInst + 1);
    ////                    int* tarSrc = (int*)(tarInst + 1);

    ////                    *tarSrc = (((int)injInst + 5) + *injSrc) - ((int)tarInst + 5);
    ////#else
    ////                    Console.WriteLine("\nVersion x64 Release\n");
    ////                    *tar = *inj;
    ////#endif
    ////                }
    ////            }
    ////        }
    ////    }

    //    public class Test3
    //    {
    //        private readonly IServiceScopeFactory serviceScopeFactory;

    //        public Test3(IServiceScopeFactory serviceScopeFactory)
    //        {
    //            this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
    //        }
    //    }

    //    public interface IMygenericTest<T>
    //    {

    //    }

    //    public class MyGenericTest<T> : IMygenericTest<T>
    //    {

    //    }

    [TestClass]
    public class UnitTest1
    {

        [TestMethod]
        public void Test1()
        {
            var container = new UnityContainer();
            container.AddExtension(new EnumerableExtension());
            // container.RegisterInstance<IStartupConfigureContainerFilter<IUnityContainer>>();
            // container.RegisterInstance<string>("test", "test");

            var strings = container.Resolve<IEnumerable<string>>().ToArray();

            Assert.AreEqual(0, strings.Length);


            var filters = container.Resolve<IEnumerable< IStartupConfigureContainerFilter<IUnityContainer>>>().ToArray();

            Assert.AreEqual(0, filters.Length);

        }

        [TestMethod]
        public void Test2_IsOk()
        {

            var serviceColection = new ServiceCollection();
            serviceColection.AddHttpClient();

            var container = new UnityContainer().CreateChildContainer();
           

            //var factory = new ServiceProviderFactory(options => options.With(container));

            //var sp = factory.CreateServiceProvider(serviceColection);

            //var httpFactory = sp.GetRequiredService<IHttpClientFactory>();

            //var client = httpFactory.CreateClient();


        }

        public class DummyService
        {
            private readonly IServiceProvider serviceProvider;
            private readonly IHttpClientFactory httpClientFactory;

            public DummyService(IServiceProvider serviceProvider, IHttpClientFactory httpClientFactory)
            {
                this.serviceProvider = serviceProvider;
                this.httpClientFactory = httpClientFactory;
            }

            public HttpClient CreateClient()
            {
                return this.httpClientFactory.CreateClient();
            }
        }

        [TestMethod]
        public void Test2_failing()
        {

            var serviceColection = new ServiceCollection();

           
            serviceColection.AddHttpClient();

            var container = new UnityContainer().CreateChildContainer();


            //var factory = new ServiceProviderFactory(options => options.With(container));

            //var sp = factory.CreateServiceProvider(serviceColection);
            //var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
            //using (var scope = scopeFactory.CreateScope())
            //{
            //    var httpFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();

            //    var sp1 = httpFactory.GetType().GetField("_services", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(httpFactory) as IServiceProvider;
            //    var c1 = sp1.GetType().GetField("_container", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(sp1);
            //    Assert.IsNotNull(c1,"In scope");

            //}

            //{
            //    var httpFactory = sp.GetRequiredService<IHttpClientFactory>();
            //    var sp1 = httpFactory.GetType().GetField("_services", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(httpFactory) as IServiceProvider;
            //    var c1 = sp1.GetType().GetField("_container", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(sp1);
            //    Assert.IsNotNull(c1, "after scope");
            //}

        }
        [TestMethod]
        public void Test2()
        {

            var serviceColection = new ServiceCollection();
           
            serviceColection.AddTransient<DummyService>();
            serviceColection.AddHttpClient();

            var container = new UnityContainer().CreateChildContainer();


            //var factory = new ServiceProviderFactory(options => options.With(container));

            //var sp = factory.CreateServiceProvider(serviceColection);
            //var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
            //using (var scope = scopeFactory.CreateScope())
            //{
            //    var httpFactory1 = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
            //    //  var httpFactory = sp.GetRequiredService<IHttpClientFactory>();
            //    var subclient = scope.ServiceProvider.GetRequiredService<DummyService>().CreateClient();
            //}

           
            //var httpFactory = sp.GetRequiredService<IHttpClientFactory>();
            //var client = httpFactory.CreateClient();

           
             

        
            //using (var scope = scopeFactory.CreateScope())
            //{
            //    var subclient = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient();
            //}

            //client = httpFactory.CreateClient();

         
        }




        //        [TestMethod]
        //        public void Test1()
        //        {
        //            var container = new UnityContainer();
        //            container.RegisterType<Test1>(new ContainerControlledLifetimeManager());
        //            container.RegisterType<ITest1, Test1>();
        //            container.RegisterType<ITest2, Test1>();

        //            var a = container.Resolve<ITest1>();
        //            var b = container.Resolve<ITest1>();

        //            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());



        //        }
        //        [TestMethod]
        //        public void Test2()
        //        {
        //            var container = new UnityContainer();
        //            container.RegisterType<Test1>(new ContainerControlledLifetimeManager());
        //            container.RegisterType<ITest1, Test1>();
        //            container.RegisterType<ITest2, Test1>();

        //            var child = container.CreateChildContainer();

        //            var a = child.Resolve<ITest1>();
        //            var b = child.Resolve<ITest1>();

        //            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());



        //        }
        //        internal class MyOptionsExtension : UnityContainerExtension
        //        {
        //            protected override void Initialize()
        //            {

        //            }

        //            public ILifetimeContainer Lifetime => Context.Lifetime;
        //        }

        //        [TestMethod]
        //        public void Test3()
        //        {
        //            var container = new UnityContainer().AddExtension(new MyOptionsExtension());

        //            var lifetime=container.Configure< MyOptionsExtension>().Lifetime;

        //            container.RegisterType<Test1>(new HierarchicalLifetimeManager());
        //            container.RegisterType<ITest1, Test1>(new HierarchicalLifetimeManager());
        //            container.RegisterType<ITest2, Test1>(new HierarchicalLifetimeManager());


        //            container.RegisterType(typeof(IOptions<>), typeof(OptionsManager<>), new InjectionSingletonLifetimeManager(lifetime))
        //              .RegisterType(typeof(IOptionsSnapshot<>), typeof(OptionsManager<>), new HierarchicalLifetimeManager())
        //         .RegisterType(typeof(IOptionsMonitor<>), typeof(OptionsMonitor<>), new InjectionSingletonLifetimeManager(lifetime))
        //          .RegisterType(typeof(IOptionsFactory<>), typeof(OptionsFactory<>), new TransientLifetimeManager())
        //         .RegisterType(typeof(IOptionsMonitorCache<>), typeof(OptionsCache<>), new InjectionSingletonLifetimeManager(lifetime));

        //            {
        //                var child = container.CreateChildContainer();

        //                var a = child.Resolve<ITest1>();
        //                var b = child.Resolve<ITest2>();

        //                Assert.AreEqual(a.GetHashCode(), b.GetHashCode());

        //                child.Dispose();
        //            }
        //            {
        //                var child = container.CreateChildContainer();

        //                var a = child.Resolve<ITest1>();
        //                var b = child.Resolve<ITest2>();

        //                Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        //            }

        //        }

        //        [TestMethod]
        //        public void Test6()
        //        {
        //            var configuration = new ConfigurationBuilder().AddEnvironmentVariables().AddInMemoryCollection(new Dictionary<string, string> { { "KeyVault:test:foo","foo"} ,{"KeyVault:PimetrServiceBus","foo" } }).Build();

        //            var section = configuration.GetSection("KeyVault");
        //            Assert.IsNotNull(section);
        //            var test = section.GetSection("test");
        //            Assert.IsNotNull(section);
        //            var foo = test["foo"];
        //            Assert.IsNotNull(section);
        //            Assert.AreEqual("foo", foo);
        //            Assert.AreEqual("foo", configuration.GetSection("KeyVault:test:foo").Value);
        //            Assert.AreEqual("foo", test.GetValue<string>("foo"));

        //            Assert.AreEqual("foo", configuration.GetValue<string>("KeyVault:test:foo"));

        //            Assert.AreEqual("foo", configuration.GetSection("KeyVault:PimetrServiceBus").Value);

        //        }
        //        [TestMethod]
        //        public void Test5()
        //        {
        //            var container = new FabricContainer();

        //            var child = container.IntializeScope();
        //            child.RegisterType<Test3>();

        //            var test= child.Resolve<Test3>();
        //            Assert.IsNotNull(test);

        //        }
        //        [TestMethod]
        //        public void Test4()
        //        {
        //            var serviceColection = new ServiceCollection();
        //            serviceColection.AddOptions();

        //            var container = new UnityContainer();
        //            container.RegisterType<Test1>(new HierarchicalLifetimeManager());
        //            container.RegisterType<ITest1, Test1>(new HierarchicalLifetimeManager());
        //            container.RegisterType<ITest2, Test1>(new HierarchicalLifetimeManager());

        //            var factory = new ServiceProviderFactory(container);

        //            var childcontainer = factory.CreateBuilder(serviceColection) ;

        //            {
        //                var child = childcontainer.CreateChildContainer();

        //                var a = child.Resolve<ITest1>();
        //                var b = child.Resolve<ITest2>();

        //                Assert.AreEqual(a.GetHashCode(), b.GetHashCode());

        //                child.Dispose();
        //            }
        //            {
        //                var child = childcontainer.CreateChildContainer();

        //                var a = child.Resolve<ITest1>();
        //                var b = child.Resolve<ITest2>();

        //                Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        //            }

        //        }

        //        [TestMethod]
        //        public void ShouldUseDefaultValue()
        //        {
        //            //My own implementation, works
        //         //   var container = new UnityContainer().WithAspNetCoreServiceProvider();
        //          //  container.Resolve<MyTestClass>();



        //            var c = new UnityContainer();



        //            var s1 = new ServiceCollection().AddOptions();
        //            var s2 = new ServiceCollection().AddOptions();

        //            s1.AddSingleton < ITest1,Test1>();
        //            s2.AddSingleton<ITest1, Test1>();

        //            var spf1 = new ServiceProviderFactory(c);
        //            var spf2 = new ServiceProviderFactory(c);

        //            var cc1 = spf1.CreateBuilder(s1);
        //            var cc2 = spf2.CreateBuilder(s2);

        //            var sp1 = spf1.CreateServiceProvider(cc1);
        //            var t1 = sp1.GetRequiredService<ITest1>();

        //            var sp2 = spf2.CreateServiceProvider(cc2);
        //            var t2 = sp2.GetRequiredService<ITest1>();



        //            Assert.AreNotSame(t2, t1);

        //            // var sp =spf.CreateServiceProvider(cc);

        //            //  Assert.IsNotNull(sp.GetService<MyTestClass1>());
        //            //  var sp = cc.Resolve<IServiceProvider>();


        //            //  var ccc = cc.CreateChildContainer();
        //            // var spp = ccc.Resolve<IServiceProvider>();

        //            //cc.Resolve<MyTestClass>();
        //        }
        //        public class t : Hub
        //        {

        //        }
        //        [TestMethod]
        //        public void Test9()
        //        {


        //            var c = new UnityContainer();



        //            var s1 = new ServiceCollection().AddOptions();
        //            var s2 = new ServiceCollection().AddOptions();


        //            s1.AddSingleton<ITest1, Test1>();
        //            s2.AddSingleton<ITest1, Test1>();
        //            s1.AddSingleton(typeof(IMygenericTest<>), typeof(MyGenericTest<>));
        //            s2.AddSingleton(typeof(IMygenericTest<>), typeof(MyGenericTest<>));


        //            var sp1 = s1.BuildServiceProvider(c.CreateChildContainer());

        //            var t1 = sp1.GetRequiredService<ITest1>();

        //            var sp2 = s2.BuildServiceProvider(c.CreateChildContainer());
        //            var t2 = sp2.GetRequiredService<ITest1>();

        //            //Works for above, but not below


        //            Assert.AreNotSame(t2, t1);
        //            Assert.AreNotSame(t2.GetHashCode(), t1.GetHashCode());

        //            var h1 = sp1.GetRequiredService<IMygenericTest<int>>();
        //            var h2 = sp2.GetRequiredService<IMygenericTest<int>>();
        //            Assert.IsNotNull(h1, "notnull");
        //            Assert.AreNotSame(h1, h2, "sameobj");


        //        }

        //        [TestMethod]
        //        public void Test13()
        //        {

        //          //  ReplaceTest.Install();

        //            var c = new UnityContainer();
        //            var cc = c.CreateChildContainer();
        //            cc.AddNewExtension< IOptionsExtension > ();

        //            var sc = new ServiceCollection();
        //            sc.AddIdentityServer(o =>
        //            {
        //                o.Events.RaiseErrorEvents = true;
        //            });

        //            var sp = sc.BuildServiceProvider();


        //            var test =  sp.GetService<IdentityServerOptions>();

        //            Assert.IsNotNull(test); //Passes


        //            var sp1 = sc.BuildServiceProvider(cc.CreateChildContainer());


        //            var test1 = sp1.GetService<IdentityServerOptions>();

        //            Assert.IsNotNull(test1);

        //        }


        //        [TestMethod]
        //        public void Test10()
        //        {


        //            var c = new UnityContainer();

        //            var c1 = c.CreateChildContainer();
        //            var c2 = c.CreateChildContainer();

        //            c1.RegisterType(typeof(IMygenericTest<>), typeof(MyGenericTest<>),new ContainerControlledLifetimeManager());


        //            var t1 = c1.Resolve<IMygenericTest<int>>();
        //            Assert.IsNotNull(t1);

        //            c2.RegisterType(typeof(IMygenericTest<>), typeof(MyGenericTest<>), new ContainerControlledLifetimeManager());


        //            var t2 = c2.Resolve<IMygenericTest<int>>();
        //            Assert.IsNotNull(t2);

        //            Assert.AreNotSame(t2, t1);

        //        }

        //        [TestMethod]
        //        public void Test8()
        //        {
        //            //My own implementation, works
        //            //   var container = new UnityContainer().WithAspNetCoreServiceProvider();
        //            //  container.Resolve<MyTestClass>();



        //            var c = new UnityContainer();



        //            var s1 = new ServiceCollection().AddOptions();
        //            var s2 = new ServiceCollection().AddOptions();
        //            s1.AddSignalRCore();
        //            s2.AddSignalRCore();

        //            s1.AddSingleton<ITest1, Test1>();
        //            s2.AddSingleton<ITest1, Test1>();

        //            var sp1 = s1.BuildServiceProvider(c);

        //             var t1 = sp1.GetRequiredService<ITest1>();

        //            var sp2 = s2.BuildServiceProvider(c);
        //            var t2 = sp2.GetRequiredService<ITest1>();

        //            //Works for above, but not below


        //            Assert.AreNotSame(t2, t1);
        //            Assert.AreNotSame(t2.GetHashCode(), t1.GetHashCode());

        //            var h1 = sp1.GetRequiredService<IHubContext<t>>();
        //            var h2 = sp2.GetRequiredService<IHubContext<t>>();

        //            Assert.AreNotSame(h1, h2,"sameobj");
        //            Assert.AreNotSame(h1.GetHashCode(), h2.GetHashCode(), "samehash");

        //            // var sp =spf.CreateServiceProvider(cc);

        //            //  Assert.IsNotNull(sp.GetService<MyTestClass1>());
        //            //  var sp = cc.Resolve<IServiceProvider>();


        //            //  var ccc = cc.CreateChildContainer();
        //            // var spp = ccc.Resolve<IServiceProvider>();

        //            //cc.Resolve<MyTestClass>();
        //        }


        //        [TestMethod]
        //        public void TestChilds()
        //        {
        //            {
        //                var root = new UnityContainer();
        //                root.RegisterType<AFactory>(new ContainerControlledLifetimeManager());
        //                root.RegisterType<IAFactory, AFactory>();


        //                var first = root.Resolve<IAFactory>();
        //                var second = root.Resolve<IAFactory>();

        //                Assert.AreEqual(first, second);

        //                var child = root.CreateChildContainer();

        //                var third = child.Resolve<IAFactory>();
        //                Assert.AreEqual(first, third);
        //            }
        //            // third.Disposed = true;

        //            {
        //                var root = new UnityContainer();
        //                root.RegisterType<AFactory>(new ContainerControlledLifetimeManager());
        //                root.RegisterType<IAFactory, AFactory>();






        //                var child1 = root.CreateChildContainer();


        //                var first = child1.Resolve<IAFactory>();
        //                var child2 = root.CreateChildContainer();

        //                var second = child2.Resolve<IAFactory>();
        //                Assert.AreEqual(first, second);

        //                child1.Dispose();

        //                global::Unity.Microsoft.DependencyInjection.ServiceProvider.ConfigureServices(new ServiceCollection());


        //            }
        //            {
        //                var root = new UnityContainer();
        //                root.RegisterType<AFactory>(new ContainerControlledLifetimeManager());
        //                root.RegisterType<IAFactory, AFactory>();

        //                var fac = new ServiceProviderFactory(root);
        //                var child = fac.CreateBuilder(new ServiceCollection());
        //                var sp = fac.CreateServiceProvider(child);

        //                sp.GetRequiredService<IAFactory>();

        //                if(sp is IDisposable dis)
        //                {
        //                    dis.Dispose();
        //                }

        //                var first = root.Resolve<IAFactory>();
        //                var second = root.Resolve<IAFactory>();

        //                Assert.AreEqual(first, second);


        //            }
        //            {

        //                var test = new UnityContainer();
        //            }


        //        }

        //    }

        //    public class TestStartup
        //    {

        //    }

        //    public interface IAFactory : IDisposable
        //    {
        //        bool Disposed { get; set; }
        //    }
        //    public class AFactory : IAFactory, IDisposable
        //    {
        //        public Guid Id { get; set; } = Guid.NewGuid();

        //        public bool Disposed { get; set; } = false;

        //        public void Dispose()
        //        {
        //            throw new NotImplementedException();
        //        }
    }
}
