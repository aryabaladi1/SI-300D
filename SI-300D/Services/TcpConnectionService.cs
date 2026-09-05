using SI_300D.Models;
using SI_300D.Services.Windows;
using System.Net.NetworkInformation;

namespace SI_300D.Services
{
    public class TcpConnectionService
    {
        private readonly TcpTableService _tcpTableService;
        private readonly ProcessService _processService;

        public TcpConnectionService()
        {
            _tcpTableService = new TcpTableService();
            _processService = new ProcessService();
        }

        public List<TcpConnection> GetActiveConnections()
        {
            var entries = _tcpTableService.GetTcpTable();

            var connections = new List<TcpConnection>();

            foreach (var entry in entries)
            {
                var processName = _processService.GetProcessName(entry.ProcessId);

                connections.Add(new TcpConnection
                {
                    LocalAddress = entry.LocalAddress,
                    LocalPort = entry.LocalPort,
                    RemoteAddress = entry.RemoteAddress,
                    RemotePort = entry.RemotePort,
                    State = (TcpState)entry.State,
                    ProcessId = entry.ProcessId,
                    ProcessName = processName
                });
            }

            return connections;
        }
    }
}