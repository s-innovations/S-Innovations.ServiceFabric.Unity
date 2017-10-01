using Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SInnovations.Unity.AspNetCore
{
    public static class UnityTypeExtension
    {
        public static bool CanResolve(this IUnityContainer container, Type type)
        {
            if (type.IsClass)
                return true;

            if (type.IsGenericType)
            {
                var gerericType = type.GetGenericTypeDefinition();
                if (gerericType == typeof(IEnumerable<>) ||
                    gerericType.IsClass ||
                    container.IsRegistered(gerericType))
                {
                    return true;
                }
            }

            return container.IsRegistered(type);
        }
    }
}
