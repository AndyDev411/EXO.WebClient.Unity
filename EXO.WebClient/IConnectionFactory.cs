using EXO.Networking.Common;

namespace EXO.WebClient
{
    public interface IConnectionFactory
    {
        public IConnection CreateConnection();
    }
}
