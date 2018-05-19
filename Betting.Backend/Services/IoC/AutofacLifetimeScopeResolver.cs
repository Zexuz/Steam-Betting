using Autofac;
using Betting.Backend.Services.Interfaces.IoC;

namespace Betting.Backend.Services.IoC
{
    public class AutofacLifetimeScopeResolver : ILifetimeScopeResolver
    {
        private readonly ILifetimeScope _scope;

        public AutofacLifetimeScopeResolver(ILifetimeScope scope)
        {
            _scope = scope;
        }

        public T Resolve<T>()
        {
            return _scope.Resolve<T>();
        }

        public void Dispose()
        {
            _scope.Dispose();
        }
    }
}