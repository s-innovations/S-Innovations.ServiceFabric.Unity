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

- [] Verify that the above example of what happens is still accurate after all the refactoring.
- [x] Remove dependency on unity. Note: Will not happen, as IOC of dotnet core do not support child containers. Therefore we must bring our own container. I been using Unity for all the time I remember, so I will be sticking with this. Consider abstractin it out so you also can bring your own. For now there is no arguments to change this.
- [x] Use CoreCLR dependency injection instead. Note: Its been integrated such IServiceProviderFactory is used. Meaning that aspnet core apps can use the container and registrations to their apps also.
- [x] When dotnet core 1.1.0 is out, its possible to make services go from main.cs to startup.cs. All servics registered in main can now be used in nested services, like dotnet core apps.


## Examples

### S-Innovations Gateway
In S-Innovations Gateway the dependency injection is used for greating a gateway application using nginx, that allows hosting of microservices on dotnet core in service fabric.
It uses letsencrypt to automatically set up certificates and ssloffloading for backend services.
TODO:
- [] Insert URL

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

and the host service that uses a startup class.

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

        #region StatelessService


        protected override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(this);
            base.ConfigureServices(services);
        }

        private bool IsNginxRunning(){...}

        private async Task WriteConfigAsync(CancellationToken token)
        {
            var endpoint = FabricRuntime.GetActivationContext().GetEndpoint("NginxServiceEndpoint");
            var sslEndpoint = FabricRuntime.GetActivationContext().GetEndpoint("NginxSslServiceEndpoint");

            var sb = new StringBuilder();

            sb.AppendLine("worker_processes  1;");
            sb.AppendLine("events {\n\tworker_connections  1024;\n}");
            sb.AppendLine("http {");

            File.WriteAllText("mime.types", WriteMimeTypes(sb, "mime.types").ToString());

            sb.AppendLine("\tkeepalive_timeout  65;");
            sb.AppendLine("\tgzip  on;");
            sb.AppendLine("\tproxy_buffer_size   128k;");
            sb.AppendLine("\tproxy_buffers   4 256k;");
            sb.AppendLine("\tproxy_busy_buffers_size   256k;");           
            
            {
                var proxies = await GetGatewayServicesAsync(token);

                foreach (var serverGroup in proxies.GroupByServerName())
                {
                    var serverName = serverGroup.Key;
                    var sslOn = serverName != "localhost" && serverGroup.Value.Any(k => k.Ssl.Enabled);

                    if (sslOn)
                    {
                        var state = await GetCertGenerationStateAsync(serverName, serverGroup.Value.First().Ssl, token);
                        sslOn = state != null && state.Completed;
                    }


                    sb.AppendLine("\tserver {");
                    {
                        sb.AppendLine($"\t\tlisten       {endpoint.Port};");
                        if (sslOn)
                        {
                            sb.AppendLine($"\t\tlisten       {sslEndpoint.Port} ssl;");
                        }

                        sb.AppendLine($"\t\tserver_name  {serverName};");
                        sb.AppendLine();

                        if (sslOn)
                        {

                            var certs = storageAccount.CreateCloudBlobClient().GetContainerReference("certs");

                            var certBlob = certs.GetBlockBlobReference($"{serverName}.crt");
                            var keyBlob = certs.GetBlockBlobReference($"{serverName}.key");

                            Directory.CreateDirectory(Path.Combine(Context.CodePackageActivationContext.WorkDirectory, "letsencrypt"));

                            await certBlob.DownloadToFileAsync($"{Context.CodePackageActivationContext.WorkDirectory}/letsencrypt/{serverName}.crt", FileMode.Create);
                            await keyBlob.DownloadToFileAsync($"{Context.CodePackageActivationContext.WorkDirectory}/letsencrypt/{serverName}.key", FileMode.Create);


                            sb.AppendLine($"\t\tssl_certificate {Context.CodePackageActivationContext.WorkDirectory}/letsencrypt/{serverName}.crt;");
                            sb.AppendLine($"\t\t ssl_certificate_key {Context.CodePackageActivationContext.WorkDirectory}/letsencrypt/{serverName}.key;");

                        }


                        foreach (var a in serverGroup.Value)
                        {
                            if (a.IPAddressOrFQDN == this.Context.NodeContext.IPAddressOrFQDN)
                            {
                                WriteProxyPassLocation(2, a.ReverseProxyLocation, a.BackendPath, sb);
                            }
                        }


                    }
                    sb.AppendLine("\t}");
                }

            }
            sb.AppendLine("}");

            File.WriteAllText("nginx.conf", sb.ToString());
        }



        private static StringBuilder WriteMimeTypes(StringBuilder sb, string name){...}

        private static void WriteProxyPassLocation(int level, string location, string url, StringBuilder sb)
        {

            var tabs = string.Join("", Enumerable.Range(0, level + 1).Select(r => "\t"));
            sb.AppendLine($"{string.Join("", Enumerable.Range(0, level).Select(r => "\t"))}location {location} {{");
            {
                sb.AppendLine($"{tabs}proxy_pass {url.TrimEnd('/')}/;");
                //  sb.AppendLine($"{tabs}proxy_redirect off;");
                sb.AppendLine($"{tabs}server_name_in_redirect on;");
                sb.AppendLine($"{tabs}port_in_redirect off;");


                sb.AppendLine($"{tabs}proxy_set_header Upgrade $http_upgrade;");
                sb.AppendLine($"{tabs}proxy_set_header Connection keep-alive;");

                sb.AppendLine($"{tabs}proxy_set_header Host					  $host;");
                sb.AppendLine($"{tabs}proxy_set_header X-Real-IP              $remote_addr;");
                sb.AppendLine($"{tabs}proxy_set_header X-Forwarded-For        $proxy_add_x_forwarded_for;");
                sb.AppendLine($"{tabs}proxy_set_header X-Forwarded-Host       $host;");
                sb.AppendLine($"{tabs}proxy_set_header X-Forwarded-Server     $host;");
                sb.AppendLine($"{tabs}proxy_set_header X-Forwarded-Proto      $scheme;");
                sb.AppendLine($"{tabs}proxy_set_header X-Forwarded-Path       $request_uri;");
                if (!location.Trim().StartsWith("~"))
                    sb.AppendLine($"{tabs}proxy_set_header X-Forwarded-PathBase   {location};");

                sb.AppendLine($"{tabs}proxy_cache_bypass $http_upgrade;");
            }
            sb.AppendLine($"{string.Join("", Enumerable.Range(0, level).Select(r => "\t"))}}}");



        }



        private void LaunchNginxProcess(string arguments){...}


        protected override Task OnCloseAsync(CancellationToken cancellationToken){...}      
       
        private DateTimeOffset lastWritten = DateTimeOffset.MinValue;
        public async Task DeleteGatewayServiceAsync(string v, CancellationToken cancellationToken)
        {
            var applicationName = this.Context.CodePackageActivationContext.ApplicationName;
            var actorServiceUri = new Uri($"{applicationName}/GatewayServiceManagerActorService");
            List<long> partitions = await GetPartitionsAsync(actorServiceUri);
            var serviceProxyFactory = new ServiceProxyFactory();

          
            foreach (var partition in partitions)
            {
                var actorService = serviceProxyFactory.CreateServiceProxy<IGatewayServiceManagerActorService>(actorServiceUri, new ServicePartitionKey(partition));
                await actorService.DeleteGatewayServiceAsync(v,cancellationToken);
            }

        }
        public async Task<List<GatewayServiceRegistrationData>> GetGatewayServicesAsync(CancellationToken cancellationToken)
        {
            var applicationName = this.Context.CodePackageActivationContext.ApplicationName;
            var actorServiceUri = new Uri($"{applicationName}/GatewayServiceManagerActorService");
            List<long> partitions = await GetPartitionsAsync(actorServiceUri);

            var serviceProxyFactory = new ServiceProxyFactory();

            var all = new List<GatewayServiceRegistrationData>();
            foreach (var partition in partitions)
            {
                var actorService = serviceProxyFactory.CreateServiceProxy<IGatewayServiceManagerActorService>(actorServiceUri, new ServicePartitionKey(partition));

                var state = await actorService.GetGatewayServicesAsync(cancellationToken);
                all.AddRange(state);

            }
            return all;
        }
         
        private async Task<List<long>> GetPartitionsAsync(Uri actorServiceUri)
        {
            var partitions = new List<long>();
            var servicePartitionList = await _fabricClient.QueryManager.GetPartitionListAsync(actorServiceUri);
            foreach (var servicePartition in servicePartitionList)
            {
                var partitionInformation = servicePartition.PartitionInformation as Int64RangePartitionInformation;
                partitions.Add(partitionInformation.LowKey);
            }

            return partitions;
        }

        public async Task<CertGenerationState> GetCertGenerationStateAsync(string hostname, SslOptions options, CancellationToken token)
        {
            var applicationName = this.Context.CodePackageActivationContext.ApplicationName;
            var actorServiceUri = new Uri($"{applicationName}/GatewayServiceManagerActorService");
            List<long> partitions = await GetPartitionsAsync(actorServiceUri);

            var serviceProxyFactory = new ServiceProxyFactory();

            var actors = new Dictionary<long, DateTimeOffset>();
            foreach (var partition in partitions)
            {
                var actorService = serviceProxyFactory.CreateServiceProxy<IGatewayServiceManagerActorService>(actorServiceUri, new ServicePartitionKey(partition));

                var state = await actorService.GetCertGenerationInfoAsync(hostname, options, token);
                if (state != null && state.RunAt.HasValue && state.RunAt.Value > DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(14)))
                {
                    return state;
                }

            }

            var gateway = ActorProxy.Create<IGatewayServiceManagerActor>(new ActorId(0));
            await gateway.RequestCertificateAsync(hostname, options);

            return null;
        }
        public async Task SetLastUpdatedAsync(DateTimeOffset time, CancellationToken token)
        {
      
            var gateway = ActorProxy.Create<IGatewayServiceManagerActor>(new ActorId(0));
            await gateway.SetLastUpdatedNow();
           
        }
        public async Task<IDictionary<long, DateTimeOffset>> GetLastUpdatedAsync(CancellationToken token)
        {

            var applicationName = this.Context.CodePackageActivationContext.ApplicationName;
            var actorServiceUri = new Uri($"{applicationName}/GatewayServiceManagerActorService");
            List<long> partitions = await GetPartitionsAsync(actorServiceUri);

            var serviceProxyFactory = new ServiceProxyFactory();

            var actors = new Dictionary<long, DateTimeOffset>();
            foreach (var partition in partitions)
            {
                var actorService = serviceProxyFactory.CreateServiceProxy<IGatewayServiceManagerActorService>(actorServiceUri, new ServicePartitionKey(partition));

                var counts = await actorService.GetLastUpdatedAsync(token);
                foreach (var count in counts)
                {
                    actors.Add(count.Key, count.Value);
                }
            }
            return actors;
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
        #endregion StatelessService


    }
```

### EarthML IdentityServer
Also used in my identity server service fabric service


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