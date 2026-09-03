using SI_300D.Services;
using System.ComponentModel;
using System.Net.NetworkInformation;

namespace SI_300D.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly NetworkStatisticsService _networkStatisticsService;
        private readonly NetworkInterface? _networkInterface;
        private CancellationTokenSource? _monitoringCancellation;

        public string ApplicationName => "SI-300D";

        public double DownloadBytesPerSecond { get; private set; }

        public double UploadBytesPerSecond { get; private set; }

        public string InterfaceName { get; private set; } = string.Empty;

        public string InterfaceStatus { get; private set; } = string.Empty;

        public string InterfaceType { get; private set; } = string.Empty;

        public long InterfaceSpeed { get; private set; }

        public bool IsMonitoring { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public string DownloadSpeed => FormatBytesPerSecond(DownloadBytesPerSecond);

        public string UploadSpeed => FormatBytesPerSecond(UploadBytesPerSecond);

        public string InterfaceSpeedDisplay => FormatBitsPerSecond(InterfaceSpeed);

        public MainViewModel()
        {
            _networkStatisticsService = new NetworkStatisticsService();

            _networkInterface = NetworkInterface
                .GetAllNetworkInterfaces()
                .FirstOrDefault(networkInterface =>
                    networkInterface.OperationalStatus == OperationalStatus.Up);

            if (_networkInterface is not null)
            {
                InterfaceName = _networkInterface.Name;
                InterfaceStatus = _networkInterface.OperationalStatus.ToString();
                InterfaceType = _networkInterface.NetworkInterfaceType.ToString();
                InterfaceSpeed = _networkInterface.Speed;
            }
        }

        public async Task StartMonitoringAsync()
        {
            if (_networkInterface is null || IsMonitoring)
                return;

            _monitoringCancellation = new CancellationTokenSource();

            IsMonitoring = true;
            OnPropertyChanged(nameof(IsMonitoring));

            try
            {
                await foreach (var statistics in _networkStatisticsService.MonitorAsync(
                    _networkInterface,
                    _monitoringCancellation.Token))
                {
                    DownloadBytesPerSecond = statistics.DownloadBytesPerSecond;
                    UploadBytesPerSecond = statistics.UploadBytesPerSecond;

                    OnPropertyChanged(nameof(DownloadBytesPerSecond));
                    OnPropertyChanged(nameof(DownloadSpeed));

                    OnPropertyChanged(nameof(UploadBytesPerSecond));
                    OnPropertyChanged(nameof(UploadSpeed));
                }
            }
            catch (OperationCanceledException)
            {
                // ayo :3
            }
        }

        public void StopMonitoring()
        {
            if (!IsMonitoring)
                return;

            _monitoringCancellation?.Cancel();

            IsMonitoring = false;
            OnPropertyChanged(nameof(IsMonitoring));
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }

        private static string FormatBytesPerSecond(double bytesPerSecond)
        {
            if (bytesPerSecond < 1024)
                return $"{bytesPerSecond:N0} B/s";

            if (bytesPerSecond < 1024 * 1024)
                return $"{bytesPerSecond / 1024:N1} KB/s";

            if (bytesPerSecond < 1024 * 1024 * 1024)
                return $"{bytesPerSecond / (1024 * 1024):N1} MB/s";

            return $"{bytesPerSecond / (1024 * 1024 * 1024):N1} GB/s";
        }

        private static string FormatBitsPerSecond(long bitsPerSecond)
        {
            if (bitsPerSecond < 1_000)
                return $"{bitsPerSecond:N0} bps";

            if (bitsPerSecond < 1_000_000)
                return $"{bitsPerSecond / 1_000.0:N1} Kbps";

            if (bitsPerSecond < 1_000_000_000)
                return $"{bitsPerSecond / 1_000_000.0:N1} Mbps";

            return $"{bitsPerSecond / 1_000_000_000.0:N1} Gbps";
        }
    }
}