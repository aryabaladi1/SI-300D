namespace SI_300D.Models
{
    public class NetworkInterfaceInfo
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public long Speed { get; set; }

        public List<string> IpAddresses { get; set; } = [];
    }
}
