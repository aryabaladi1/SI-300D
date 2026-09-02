using SI_300D.Models;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace SI_300D.Services
{
    public class NetworkStatisticsService
    {
        public NetworkStatistics GetStatistics(NetworkInterface networkInterface)
        {
            var statistics = networkInterface.GetIPStatistics();

            return new NetworkStatistics
            {
                BytesReceived = statistics.BytesReceived,
                BytesSent = statistics.BytesSent
            };
        }

        public NetworkStatistics CalculateRate(NetworkStatistics previous, NetworkStatistics current, double elapsedSeconds)
        {
            return new NetworkStatistics
            {
                BytesReceived = current.BytesReceived,
                BytesSent = current.BytesSent,

                DownloadBytesPerSecond =
                    (current.BytesReceived - previous.BytesReceived) / elapsedSeconds,

                UploadBytesPerSecond =
                    (current.BytesSent - previous.BytesSent) / elapsedSeconds
            };
        }

        public async IAsyncEnumerable<NetworkStatistics> MonitorAsync(NetworkInterface networkInterface, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));

            var previous = GetStatistics(networkInterface);
            var stopwatch = Stopwatch.StartNew();

            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                var current = GetStatistics(networkInterface);

                var elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
                stopwatch.Restart();

                var statistics = CalculateRate(
                    previous,
                    current,
                    elapsedSeconds);

                previous = current;

                yield return statistics;
            }
        }
    }
}
