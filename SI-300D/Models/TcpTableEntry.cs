namespace SI_300D.Models
{
    public class TcpTableEntry
    {
        public string LocalAddress { get; set; } = string.Empty;

        public int LocalPort { get; set; }

        public string RemoteAddress { get; set; } = string.Empty;

        public int RemotePort { get; set; }

        public uint State { get; set; }

        public uint ProcessId { get; set; }
    }
}
