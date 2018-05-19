using Grpc.Core;

namespace Betting.Backend.Factories
{
    public interface IGrpcConnectionFactory
    {
        Channel CreateConnection(string host, int port);
    }
}