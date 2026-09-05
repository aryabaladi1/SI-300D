using SI_300D.Models;
using SI_300D.Services.Windows;
using System.Net.NetworkInformation;

namespace SI_300D.Services
{
    public class TcpConnectionService
    {
        private readonly TcpTableService _tcpTableService;

        public TcpConnectionService()
        {
            _tcpTableService = new TcpTableService();
        }

        public List<TcpConnection> GetActiveConnections()
        {
            var entries = _tcpTableService.GetTcpTable();

            var connections = new List<TcpConnection>();

            foreach (var entry in entries)
            {
                connections.Add(new TcpConnection
                {
                    LocalAddress = entry.LocalAddress,
                    LocalPort = entry.LocalPort,
                    RemoteAddress = entry.RemoteAddress,
                    RemotePort = entry.RemotePort,
                    State = (TcpState)entry.State,
                    ProcessId = entry.ProcessId
                });
            }

            return connections;
        }
    }
}