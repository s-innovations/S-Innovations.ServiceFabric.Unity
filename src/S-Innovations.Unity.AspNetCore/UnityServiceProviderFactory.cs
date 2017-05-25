using Microsoft.Extensions.DependencyInjection;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SInnovations.Unity.AspNetCore
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
        private HashSet<ServiceDescriptor> _descriptors = new HashSet<ServiceDescriptor>();
        public IServiceProvider CreateServiceProvider(IServiceCollection containerBuilder)
        {
            foreach (var desc in containerBuilder)
            {
                if (!_descriptors.Contains(desc))
                    _descriptors.Add(desc);
            }
            return root.Populate(_descriptors).Resolve<IServiceProvider>();

        }
    }
}
