using System.Net.NetworkInformation;

namespace SI_300D.Models
{
    public class TcpConnection
    {
        public string LocalAddress { get; set; } = string.Empty;

        public int LocalPort { get; set; }

        public string RemoteAddress { get; set; } = string.Empty;

        public int RemotePort { get; set; }

        public TcpState State { get; set; }

        public int? ProcessId { get; set; }

        public string ProcessName { get; set; } = string.Empty;
    }
}
