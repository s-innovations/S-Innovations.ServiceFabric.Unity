[![Visual Studio Team services](https://img.shields.io/vso/build/sinnovations/40c16cc5-bf99-47d4-a814-56c38cc0ea24/23.svg?style=flat-square&label=build:%20ServiceFabric.DependencyInjection)]()
[![Myget Version](http://img.shields.io/myget/s-innovations/vpre/S-Innovations.ServiceFabric.Unity.svg?style=flat-square&label=myget:%20ServiceFabric.DependencyInjection)](https://www.myget.org/feed/s-innovations/package/nuget/S-Innovations.ServiceFabric.Unity)

# Dependency Injection Sample for Servicefabric

The current sample uses Unity, but the goal is to move away from a specific container and use the CoreCLR dependency injection abstractions.

## What will happen in the sample?


1. A `MyTestActor(id="MyCoolActor")` will be created, activated and 10secs later it will ask `MySecondTestActor` to do some work.
2. The MyTestActor(id="MyCoolActor") will be garbage collected after 120 seconds, see the registration in program.cs
3. The MyTestActor(id="MyCoolActor") will be recreated, activated again each 3min and do the same as in 1.
4. The MySecondTestActor(id="MyCoolActor") will be garbage collected after 30sec and recreated every 1 min due to its own reminder or every 3min due to the dowork call in 1.
5. The MySecondTestActor(id="MyCoolActor") will every 1 min ask DependencyInjectionActorSample for its count, increase it by one and set it again.


## Dependencies

Every new actor or service created will have a scoped container, such all scoped dependency registrations will be disposed if implementing IDisposable when the actor is garbage collected.

### Scoped Lifetime

Extension methods to the unity container has been made such the same methods that are in dotnet core also exists on IUnityContainer - this way it should be straint forward to use.

## TODO

- [ ] Verify that the above example of what happens is still accurate after all the refactoring.
- [x] Remove dependency on unity. Note: Will not happen, as IOC of dotnet core do not support child containers. Therefore we must bring our own container. I been using Unity for all the time I remember, so I will be sticking with this. Consider abstractin it out so you also can bring your own. For now there is no arguments to change this.
- [x] Use CoreCLR dependency injection instead. Note: Its been integrated such IServiceProviderFactory is used. Meaning that aspnet core apps can use the container and registrations to their apps also.
- [x] When dotnet core 1.1.0 is out, its possible to make services go from main.cs to startup.cs. All servics registered in main can now be used in nested services, like dotnet core apps.


## Examples

### S-Innovations Gateway
In S-Innovations Gateway the dependency injection is used for greating a gateway application using nginx, that allows hosting of microservices on dotnet core in service fabric.
It uses letsencrypt to automatically set up certificates and ssloffloading for backend services. 

Below there is also two examples of using the gateway + dependency injection.

TODO:
- [ ] Insert URL

```
public class Program
    {

        public static void Main(string[] args)
        {


            using (var container = new UnityContainer().AsFabricContainer())
            {
                container.AddOptions();
                container.ConfigureSerilogging(logConfiguration =>
                         logConfiguration.MinimumLevel.Debug()
                         .Enrich.FromLogContext()
                         .WriteTo.LiterateConsole(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message}{NewLine}{Exception}{NewLine}")
                         .WriteTo.ApplicationInsightsTraces("10e77ea7-1d38-40f7-901c-ef3c2e7d48ef", Serilog.Events.LogEventLevel.Information));



                container.ConfigureApplicationStorage();


                var keyvaultINfo = container.Resolve<KeyVaultSecretManager>();
                var configuration = new ConfigurationBuilder()
                    .AddAzureKeyVault(keyvaultINfo.KeyVaultUrl, keyvaultINfo.Client, keyvaultINfo)
                    .Build(container);

                container.Configure<KeyVaultOptions>("KeyVault");

                container.WithLetsEncryptService(new LetsEncryptServiceOptions
                {
                    BaseUri = "https://acme-v01.api.letsencrypt.org"
                });

                container.WithStatelessService<NginxGatewayService>("GatewayServiceType");
                container.WithStatelessService<ApplicationStorageService>("ApplicationStorageServiceType");

                container.WithActor<GatewayServiceManagerActor, GatewayServiceManagerActorService>((context, actorType, factory) => new GatewayServiceManagerActorService(context, actorType, factory));


                Thread.Sleep(Timeout.Infinite);
            }


        }


    }
```

and the host service that uses a startup class, but also runs some service logic.

```
 public sealed class NginxGatewayService : KestrelHostingService<Startup>
    {
        

        public NginxGatewayService(StatelessServiceContext serviceContext, IUnityContainer container, ILoggerFactory factory, StorageConfiguration storage)
            : base(new KestrelHostingServiceOptions
            {
                GatewayOptions = new GatewayOptions
                {
                    Key = "NGINX-MANAGER",
                    ReverseProxyLocation = "/manage/",
                    ServerName = "www.earthml.com local.earthml.com",
                    Ssl = new SslOptions
                    {
                        Enabled = true,
                        SignerEmail = "info@earthml.com"
                    }
                }

            }, serviceContext, factory, container)
        {
           ...
        }
		  

        protected override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(this);
            base.ConfigureServices(services);
        }

		protected override async Task RunAsync(CancellationToken cancellationToken)
        {


            try
            {

                storageAccount = await Storage.GetApplicationStorageAccountAsync();

                var gateway = ActorProxy.Create<IGatewayServiceManagerActor>(new ActorId(0));
                var a = await _fabricClient.ServiceManager.GetServiceDescriptionAsync(this.Context.ServiceName) as StatelessServiceDescription;

                await gateway.SetupStorageServiceAsync(a.InstanceCount);
                await WriteConfigAsync(cancellationToken);

                LaunchNginxProcess($"-c \"{Path.GetFullPath("nginx.conf")}\"");



                while (true)
                {
                    if (cancellationToken.IsCancellationRequested)
                        LaunchNginxProcess($"-c \"{Path.GetFullPath("nginx.conf")}\" -s quit");
                    cancellationToken.ThrowIfCancellationRequested();
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

                    if (!IsNginxRunning())
                        LaunchNginxProcess($"-c \"{Path.GetFullPath("nginx.conf")}\"");

                    var allActorsUpdated = await GetLastUpdatedAsync(cancellationToken);
                    if (allActorsUpdated.ContainsKey(gateway.GetActorId().GetLongId()))
                    {
                        var updated = allActorsUpdated[gateway.GetActorId().GetLongId()];  // await gateway.GetLastUpdatedAsync();

                        if (!lastWritten.Equals(updated))
                        {
                            lastWritten = updated;
                            await WriteConfigAsync(cancellationToken);

                            LaunchNginxProcess($"-c \"{Path.GetFullPath("nginx.conf")}\" -s reload");
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogWarning(new EventId(), ex, "RunAsync Failed");
                throw;
            }




        }
     


    }
```

### EarthML IdentityServer
Also used in my identity server service fabric service, which also uses a startup class that implements IStartup.


```
 public class Program
    {
        private const string LiterateLogTemplate = "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message}{NewLine}{Exception}{NewLine}";


        public static void Main(string[] args)
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
                .AddEnvironmentVariables().Build();

            using (var container = new UnityContainer()
                       .AsFabricContainer().AddOptions()
                       .UseConfiguration(config) //willl also be set on hostbuilder
                       .ConfigureSerilogging(logConfiguration =>
                           logConfiguration.MinimumLevel.Information()
                           .Enrich.FromLogContext()
                           .WriteTo.LiterateConsole(outputTemplate: LiterateLogTemplate))
                       .ConfigureApplicationInsights())
            {

                if (args.Contains("--serviceFabric"))
                {
                    RunInServiceFabric(container);
                }
                else
                {
                    RunOnIIS(container);
                }
            }
        }

        private static void RunOnIIS(IUnityContainer container)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())                
                .ConfigureServices(services => 
                    services.AddSingleton(container))
                .UseUrls("http://localhost:2069/")
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }

        private static void RunInServiceFabric(IUnityContainer container)
        {
            container.WithServiceProxy<IApplicationStorageService>("fabric:/S-Innovations.ServiceFabric.GatewayApplication/ApplicationStorageService", listenerName: "RPC");

            container.WithKestrelHosting<Startup>("EarthML.IdentityServiceType",
                new KestrelHostingServiceOptions
                {
                    GatewayOptions = new GatewayOptions
                    {
                        Key = "EarthML.IdentityServiceType",
                        ServerName = "www.earthml.com earthml.com local.earthml.com",
                        ReverseProxyLocation = "/identity/",
                        Ssl = new SslOptions
                        {
                            Enabled = true,
                            SignerEmail = "info@earthml.com"
                        },
                    }
                });

            Thread.Sleep(Timeout.Infinite);
        }
    }

```

### EarthML Portal

The EarthML portal also uses dependency injection and configure the aspnet core application without a startup.

```

    public class OidcClientConfiguration
    {
        public string Authority { get; set; }

        [JsonProperty("client_id")]
        public string ClientId { get; set; }
    }


    public class Program
    {

        private const string LiterateLogTemplate = "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message}{NewLine}{Exception}{NewLine}";

        public static void Main(string[] args)
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
                .AddEnvironmentVariables().Build();

            using (var container = new UnityContainer()
                       .AsFabricContainer()
                       .AddOptions()
                       .UseConfiguration(config) //willl also be set on hostbuilder
                       .ConfigureSerilogging(logConfiguration =>
                           logConfiguration.MinimumLevel.Information()
                           .Enrich.FromLogContext()
                           .WriteTo.LiterateConsole(outputTemplate: LiterateLogTemplate))
                       .ConfigureApplicationInsights()
                       .Configure<OidcClientConfiguration>("OidcClientConfiguration"))
            {

                if (args.Contains("--serviceFabric"))
                {
                    RunInServiceFabric(container);
                }
                else
                {
                    RunOnIIS(container);
                }
            }
        }

        private static void RunOnIIS(IUnityContainer container)
        {
            using (var host = 
                new WebHostBuilder()
                        .UseKestrel()
                        .UseIISIntegration()
                        .UseWebRoot("artifacts/app")
                        .UseContentRoot(Directory.GetCurrentDirectory())
                        .ConfigureServices(ConfigureServices)
                        .ConfigureServices(services => services.AddSingleton<IServiceProviderFactory<IServiceCollection>>(new UnityServiceProviderFactory(container)))
                        .Configure(app =>
                        {
                            if (app.ApplicationServices.GetService<IHostingEnvironment>().IsDevelopment())
                            {
                                app.UseDeveloperExceptionPage();
                            }

                            // to do - wire in our HTTP endpoints
                            app.Use(async (ctx, next) =>
                            {
                                if (Path.GetExtension(ctx.Request.Path) == ".ts")
                                {

                                    await ctx.Response.WriteAsync(File.ReadAllText(ctx.Request.Path.Value.Substring(1)));
                                }
                                else
                                {
                                    await next();
                                }

                            });

                             
                            app.UseStaticFiles();
                            app.UseWebPages();
                        })
                    .Build())
            {

                host.Run();


            }
        }

        private static void RunInServiceFabric(IUnityContainer container)
        {
            container.WithKestrelHosting("EarthML.Mapify.PortalType",
                new KestrelHostingServiceOptions
                {
                    GatewayOptions = new GatewayOptions
                    {
                        Key = "EarthML.Mapify.PortalType",
                        ServerName = "www.gomapify.com www.mapify.dk www.mapify.nu local.earthml.com",
                        ReverseProxyLocation = "/app/",
                        Ssl = new SslOptions
                        {
                            Enabled = true,
                            SignerEmail = "info@earthml.com"
                        },
                    }
                }, (host) =>
                {
                    host
                    .ConfigureServices(ConfigureServices)
                    .Configure(app =>
                    { 
                        if (app.ApplicationServices.GetService<IHostingEnvironment>().IsDevelopment())
                        {
                            app.UseDeveloperExceptionPage();
                        }

                        app.Use((ctx, next) =>
                        {
                            var a = ctx.RequestServices.GetService<IOptions<OidcClientConfiguration>>();
                            var testa = ctx.RequestServices.GetService<IEnumerable<IConfigureOptions<OidcClientConfiguration>>>();
                            var testb = ctx.RequestServices.GetService<IConfigureOptions<OidcClientConfiguration>[]>();
                            var testc = ctx.RequestServices.GetService<IUnityContainer>();
                            var test = testc.Resolve<IEnumerable<IConfigureOptions<OidcClientConfiguration>>>().ToArray();
                            var test1 = testc.Resolve<IConfigureOptions<OidcClientConfiguration>[]>();
                            return next();
                        });

                        app.UseStaticFiles();
                        app.UseWebPages();
                    });

                });

            Thread.Sleep(Timeout.Infinite);
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddWebPages(new WebPagesOptions { RootViewName = "index", ViewsFolderName = "src" });


        }
    }
	
```