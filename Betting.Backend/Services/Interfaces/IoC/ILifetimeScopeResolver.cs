using System;

namespace Betting.Backend.Services.Interfaces.IoC
{
    public interface ILifetimeScopeResolver : IDisposable
    {
        T Resolve<T>();
    }
}