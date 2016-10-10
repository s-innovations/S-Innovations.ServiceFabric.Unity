using Microsoft.Extensions.DependencyInjection;

namespace SInnovations.ServiceFabric.DependencyInjection.Internals
{
    class OnActorDeactivateInterceptor : IActorDeactivationInterception
    {
        private readonly IServiceScope _scope;
        public OnActorDeactivateInterceptor(IServiceScope scope)
        {
            this._scope = scope;
        }

        public void Intercept()
        {
            this._scope.Dispose();
        }
    }
}
