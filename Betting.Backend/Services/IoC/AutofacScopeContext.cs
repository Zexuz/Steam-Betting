using System;
using Autofac;
using Betting.Backend.Services.Interfaces.IoC;

namespace Betting.Backend.Services.IoC
{
    public class AutofacScopeContext : IScopeContext
    {
        private readonly Func<IContainer> _container;

        public AutofacScopeContext(Func<IContainer> container)
        {
            _container = container;
        }

        public ILifetimeScopeResolver BeginLifetimeScope()
        {
            return new AutofacLifetimeScopeResolver(_container.Invoke().BeginLifetimeScope());
        }
    }
}