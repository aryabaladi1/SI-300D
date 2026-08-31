using SI_300D.Models;
using System.Net.NetworkInformation;

namespace SI_300D.Services
{
    public class NetworkInterfaceService
    {
        public void GetNetworkInterfaces()
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var networkInterface in interfaces)
            {
                Console.WriteLine(networkInterface.Name);
                Console.WriteLine(networkInterface.Description);
                Console.WriteLine(networkInterface.NetworkInterfaceType);
                Console.WriteLine(networkInterface.OperationalStatus);
                Console.WriteLine(networkInterface.Speed);
            }
        }
    }
}
