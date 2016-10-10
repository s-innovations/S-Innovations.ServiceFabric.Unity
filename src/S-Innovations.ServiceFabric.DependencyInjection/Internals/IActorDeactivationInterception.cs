using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SInnovations.ServiceFabric.DependencyInjection.Internals
{
    /// <summary>
    /// The <see cref="IActorDeactivationInterception"/> interface for defining an OnDeactivateInterception
    /// </summary>
    interface IActorDeactivationInterception
    {
        void Intercept();
    }
}
