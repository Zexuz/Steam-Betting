using System.Collections.Generic;
using Grpc.Core;

namespace Betting.Backend.Factories
{
    public class GrpcConnectionFactory : IGrpcConnectionFactory
    {
        private readonly Dictionary<string, Channel> _channels;

        public GrpcConnectionFactory()
        {
            _channels = new Dictionary<string, Channel>();
        }


        public Channel CreateConnection(string host, int port)
        {
            var key = $"{host}:{port}";
            if (_channels.ContainsKey(key))
                return _channels[key];

            var channel = new Channel(host, port, ChannelCredentials.Insecure);
            _channels[key] = channel;
            return channel;
        }
    }
}