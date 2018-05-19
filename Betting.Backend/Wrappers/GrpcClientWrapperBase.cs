using System;
using System.Threading.Tasks;
using Betting.Backend.Services.Interfaces;

namespace Betting.Backend.Wrappers
{
    public abstract class GrpcClientWrapperBase<TBase>
    {
        private readonly ILogService<TBase> _logService;

        protected GrpcClientWrapperBase(ILogService<TBase> logService)
        {
            _logService = logService;
        }

        protected async Task<T> SendGrpcAction<T>(Func<Task<T>> action, bool shouldThrowOnException = true)
        {
            try
            {
                return await action.Invoke();
            }
            catch (Exception e)
            {
                _logService.Error(null, "GRPC", e);
                if (shouldThrowOnException)
                    throw;
                return default(T);
            }
        }
    }
}