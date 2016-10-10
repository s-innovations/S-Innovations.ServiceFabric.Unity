using Microsoft.AspNetCore.Hosting;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Practices.Unity;
using SInnovations.ServiceFabric.Unity;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors;
using Microsoft.Extensions.Logging;
using Serilog;
using Microsoft.ServiceFabric.Actors.Client;

namespace DependencyInjectionServiceSample
{

   

    public class Program
    {
        // Entry point for the application.
        public static void Main(string[] args)
        {
            var log = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Trace()
                .CreateLogger();

            using (var container = new UnityContainer().AsFabricContainer())
            {
                var loggerfac = new LoggerFactory() as ILoggerFactory;
                loggerfac.AddSerilog();
                container.RegisterInstance(loggerfac);

                container.WithStatelessService<WebHostingService>("DependencyInjectionServiceSampleType");

               
                Thread.Sleep(Timeout.Infinite);

            }
        }

        /// <summary>
        /// A specialized stateless service for hosting ASP.NET Core web apps.
        /// </summary>
        internal sealed class WebHostingService : StatelessService, ICommunicationListener
        {
            private readonly string _endpointName = "ServiceEndpoint";

            private IWebHost _webHost;
            private IServiceProvider _services;

            public WebHostingService(StatelessServiceContext serviceContext, IServiceProvider services)
                : base(serviceContext)
            {
                this._services = services;
                
            }

             

            #region StatelessService

            protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
            {
                return new[] { new ServiceInstanceListener(_ => this) };
            }

            #endregion StatelessService

            #region ICommunicationListener

            void ICommunicationListener.Abort()
            {
                _webHost?.Dispose();
            }

            Task ICommunicationListener.CloseAsync(CancellationToken cancellationToken)
            {
                _webHost?.Dispose();

                return Task.FromResult(true);
            }

            protected override async Task RunAsync(CancellationToken cancellationToken)
            {
                

               

                await base.RunAsync(cancellationToken);
            }

            Task<string> ICommunicationListener.OpenAsync(CancellationToken cancellationToken)
            {
                var endpoint = FabricRuntime.GetActivationContext().GetEndpoint(_endpointName);

                string serverUrl = $"{endpoint.Protocol}://{FabricRuntime.GetNodeContext().IPAddressOrFQDN}:{endpoint.Port}";

                _webHost = new WebHostBuilder().ConfigureServices(services=> { }).UseKestrel()
                                               .UseContentRoot(Directory.GetCurrentDirectory())
                                               .UseStartup<Startup>()
                                               .UseUrls(serverUrl)
                                               .Build();

                _webHost.Start();

                return Task.FromResult(serverUrl);
            }

            #endregion ICommunicationListener
        }
    }
}
