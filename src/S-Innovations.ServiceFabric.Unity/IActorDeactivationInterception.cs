using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SInnovations.ServiceFabric.Unity
{
    /// <summary>
    /// The <see cref="IActorDeactivationInterception"/> interface for defining an OnDeactivateInterception
    /// </summary>
    public interface IActorDeactivationInterception
    {
        void Intercept();
    }
}
