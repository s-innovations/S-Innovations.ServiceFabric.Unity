using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Practices.Unity;
using Microsoft.AspNetCore.Hosting;

namespace SInnovations.ServiceFabric.Unity
{
  

    public class UnityServiceProviderFactory : IServiceProviderFactory<IServiceCollection>
    {
        private readonly IUnityContainer root;
        public UnityServiceProviderFactory(IUnityContainer root)
        {
            this.root = root;
        }
        public IServiceCollection CreateBuilder(IServiceCollection services)
        {

            //foreach (var registration in root.Registrations)
            //{
            //    try
            //    {
            //        if (registration.RegisteredType.IsGenericTypeDefinition)
            //        {
            //            services.AddTransient(registration.RegisteredType, registration.MappedToType);
            //        }
            //        else
            //        {
            //            services.AddTransient(registration.RegisteredType, (sp) => sp.GetService<IUnityContainer>().Resolve(registration.RegisteredType, registration.Name));
            //        }
            //    }catch(Exception ex)
            //    {
            //        Console.WriteLine(ex.ToString());
            //        Debug.WriteLine(ex.ToString());
            //    }
            //}

            return services;
        }

        public IServiceProvider CreateServiceProvider(IServiceCollection containerBuilder)
        {
            return root.Populate(containerBuilder).Resolve<IServiceProvider>();           
         
        }
    }
}
