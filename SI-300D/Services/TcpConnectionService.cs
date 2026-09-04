using SI_300D.Models;
using System.Net.NetworkInformation;

namespace SI_300D.Services
{
    public class TcpConnectionService
    {
        public List<TcpConnection> GetActiveConnections()
        {
            var connections = IPGlobalProperties
                .GetIPGlobalProperties()
                .GetActiveTcpConnections();

            return connections
                .Select(connection => new TcpConnection
                {
                    LocalAddress = connection.LocalEndPoint.Address.ToString(),
                    LocalPort = connection.LocalEndPoint.Port,
                    RemoteAddress = connection.RemoteEndPoint.Address.ToString(),
                    RemotePort = connection.RemoteEndPoint.Port,
                    State = connection.State
                })
                .ToList();
        }
    }
}
