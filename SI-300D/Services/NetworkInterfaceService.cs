using SI_300D.Models;
using System.Net.NetworkInformation;

namespace SI_300D.Services
{
    public class NetworkInterfaceService
    {
        public List<NetworkInterfaceInfo> GetNetworkInterfaces()
        {
            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            return networkInterfaces
                .Select(networkInterface => new NetworkInterfaceInfo
                {
                    Name = networkInterface.Name,
                    Description = networkInterface.Description,
                    Type = networkInterface.NetworkInterfaceType.ToString(),
                    Status = networkInterface.OperationalStatus.ToString(),
                    Speed = networkInterface.Speed,
                    IpAddresses = networkInterface
                        .GetIPProperties()
                        .UnicastAddresses
                        .Select(address => address.Address.ToString())
                        .ToList()
                })
                .ToList();
        }
    }
}
