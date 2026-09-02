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

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainViewModel()
        {
            _networkStatisticsService = new NetworkStatisticsService();

            _networkInterface = NetworkInterface
                .GetAllNetworkInterfaces()
                .FirstOrDefault(networkInterface =>
                    networkInterface.OperationalStatus == OperationalStatus.Up);
        }

        public async Task StartMonitoringAsync()
        {
            if (_networkInterface is null)
                return;

            _monitoringCancellation = new CancellationTokenSource();

            try
            {
                await foreach (var statistics in _networkStatisticsService.MonitorAsync(
                    _networkInterface,
                    _monitoringCancellation.Token))
                {
                    DownloadBytesPerSecond = statistics.DownloadBytesPerSecond;
                    UploadBytesPerSecond = statistics.UploadBytesPerSecond;

                    OnPropertyChanged(nameof(DownloadBytesPerSecond));
                    OnPropertyChanged(nameof(UploadBytesPerSecond));
                }
            }
            catch (OperationCanceledException)
            {
                // ayo :3
            }
        }

        public void StopMonitoring()
        {
            _monitoringCancellation?.Cancel();
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }
    }
}