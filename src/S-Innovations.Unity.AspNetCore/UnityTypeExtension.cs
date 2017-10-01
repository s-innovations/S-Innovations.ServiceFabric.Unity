using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace SInnovations.Unity.AspNetCore
{
    public static class UnityTypeExtension
    {
        public static bool CanResolve(this IUnityContainer container, Type type)
        {
            var typeInfo = type.GetTypeInfo();

            if (typeInfo.IsClass)
                return true;

           
            if (typeInfo.IsGenericType)
            {
                var gerericType = type.GetGenericTypeDefinition();
                if (gerericType == typeof(IEnumerable<>) ||
                    gerericType.GetTypeInfo().IsClass ||
                    container.IsRegistered(gerericType))
                {
                    return true;
                }
            }

            return container.IsRegistered(type);
        }
    }
}
