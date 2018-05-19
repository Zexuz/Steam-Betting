namespace Betting.Backend.Services.Interfaces.IoC
{
    public interface IScopeContext
    {
        ILifetimeScopeResolver BeginLifetimeScope();
    }
}