using SI_300D.Models;
using SI_300D.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.NetworkInformation;

namespace SI_300D.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly NetworkStatisticsService _networkStatisticsService;
        private readonly NetworkInterfaceService _networkInterfaceService;
        private readonly TcpConnectionService _tcpConnectionService;

        private NetworkInterface? _selectedNetworkInterface;
        private CancellationTokenSource? _monitoringCancellation;

        public string ApplicationName => "SI-300D";

        public double DownloadBytesPerSecond { get; private set; }

        public double UploadBytesPerSecond { get; private set; }

        public string InterfaceName { get; private set; } = string.Empty;

        public string InterfaceStatus { get; private set; } = string.Empty;

        public string InterfaceType { get; private set; } = string.Empty;

        public long InterfaceSpeed { get; private set; }

        public ObservableCollection<NetworkInterfaceInfo> NetworkInterfaces { get; } = new();

        public NetworkInterfaceInfo? SelectedNetworkInterface { get; private set; }

        public bool IsMonitoring { get; private set; }

        public bool CanStartMonitoring => !IsMonitoring;

        public bool CanStopMonitoring => IsMonitoring;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string DownloadSpeed =>
            FormatBytesPerSecond(DownloadBytesPerSecond);

        public string UploadSpeed =>
            FormatBytesPerSecond(UploadBytesPerSecond);

        public string InterfaceSpeedDisplay =>
            FormatBitsPerSecond(InterfaceSpeed);

        public string InterfaceStatusDisplay =>
            InterfaceStatus == "Up"
                ? "● Connected"
                : "○ Disconnected";

        public ObservableCollection<TcpConnection> TcpConnections { get; } = new();

        public ObservableCollection<TcpConnection> FilteredTcpConnections { get; } = new();

        private string _searchText = string.Empty;

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText == value)
                    return;

                _searchText = value;

                OnPropertyChanged(nameof(SearchText));

                FilterTcpConnections();
            }
        }
        public MainViewModel()
        {
            _networkStatisticsService = new NetworkStatisticsService();
            _networkInterfaceService = new NetworkInterfaceService();
            _tcpConnectionService = new TcpConnectionService();

            var networkInterfaces =
                _networkInterfaceService.GetNetworkInterfaces();

            foreach (var networkInterface in networkInterfaces)
            {
                NetworkInterfaces.Add(networkInterface);
            }

            var selectedInterface = NetworkInterfaces
                .FirstOrDefault(networkInterface =>
                    networkInterface.Status == "Up");

            if (selectedInterface is not null)
            {
                SelectedNetworkInterface = selectedInterface;

                _selectedNetworkInterface =
                    GetNetworkInterface(selectedInterface.Id);

                UpdateInterfaceInformation(selectedInterface);
            }
        }

        public async Task StartMonitoringAsync()
        {
            if (_selectedNetworkInterface is null || IsMonitoring)
                return;

            _monitoringCancellation = new CancellationTokenSource();

            IsMonitoring = true;

            OnPropertyChanged(nameof(IsMonitoring));
            OnPropertyChanged(nameof(CanStartMonitoring));
            OnPropertyChanged(nameof(CanStopMonitoring));

            try
            {
                await foreach (var statistics in
                    _networkStatisticsService.MonitorAsync(
                        _selectedNetworkInterface,
                        _monitoringCancellation.Token))
                {
                    DownloadBytesPerSecond =
                        statistics.DownloadBytesPerSecond;

                    UploadBytesPerSecond =
                        statistics.UploadBytesPerSecond;

                    OnPropertyChanged(
                        nameof(DownloadBytesPerSecond));

                    OnPropertyChanged(
                        nameof(DownloadSpeed));

                    OnPropertyChanged(
                        nameof(UploadBytesPerSecond));

                    OnPropertyChanged(
                        nameof(UploadSpeed));

                    RefreshTcpConnections();
                }
            }
            catch (OperationCanceledException)
            {
                // Monitoring was stopped intentionally.
            }
        }

        public void StopMonitoring()
        {
            if (!IsMonitoring)
                return;

            _monitoringCancellation?.Cancel();

            IsMonitoring = false;

            OnPropertyChanged(nameof(IsMonitoring));
            OnPropertyChanged(nameof(CanStartMonitoring));
            OnPropertyChanged(nameof(CanStopMonitoring));
        }

        public void SelectNetworkInterface(
            NetworkInterfaceInfo networkInterface)
        {
            if (IsMonitoring)
                return;

            var selectedInterface =
                GetNetworkInterface(networkInterface.Id);

            if (selectedInterface is null)
                return;

            SelectedNetworkInterface = networkInterface;
            _selectedNetworkInterface = selectedInterface;

            DownloadBytesPerSecond = 0;
            UploadBytesPerSecond = 0;

            OnPropertyChanged(
                nameof(SelectedNetworkInterface));

            OnPropertyChanged(
                nameof(DownloadBytesPerSecond));

            OnPropertyChanged(
                nameof(DownloadSpeed));

            OnPropertyChanged(
                nameof(UploadBytesPerSecond));

            OnPropertyChanged(
                nameof(UploadSpeed));

            UpdateInterfaceInformation(networkInterface);
        }

        public void RefreshTcpConnections()
        {
            var connections =
                _tcpConnectionService.GetActiveConnections();

            TcpConnections.Clear();

            foreach (var connection in connections)
            {
                TcpConnections.Add(connection);
            }

            FilterTcpConnections();
        }

        private void UpdateInterfaceInformation(
            NetworkInterfaceInfo networkInterface)
        {
            InterfaceName = networkInterface.Name;
            InterfaceStatus = networkInterface.Status;
            InterfaceType = networkInterface.Type;
            InterfaceSpeed = networkInterface.Speed;

            OnPropertyChanged(nameof(InterfaceName));
            OnPropertyChanged(nameof(InterfaceStatus));
            OnPropertyChanged(nameof(InterfaceType));
            OnPropertyChanged(nameof(InterfaceSpeed));

            OnPropertyChanged(nameof(InterfaceSpeedDisplay));
            OnPropertyChanged(nameof(InterfaceStatusDisplay));
        }

        private static NetworkInterface? GetNetworkInterface(
            string id)
        {
            return NetworkInterface
                .GetAllNetworkInterfaces()
                .FirstOrDefault(networkInterface =>
                    networkInterface.Id == id);
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }

        private static string FormatBytesPerSecond(
            double bytesPerSecond)
        {
            if (bytesPerSecond < 1024)
                return $"{bytesPerSecond:N0} B/s";

            if (bytesPerSecond < 1024 * 1024)
                return $"{bytesPerSecond / 1024:N1} KB/s";

            if (bytesPerSecond < 1024 * 1024 * 1024)
                return $"{bytesPerSecond / (1024 * 1024):N1} MB/s";

            return
                $"{bytesPerSecond / (1024 * 1024 * 1024):N1} GB/s";
        }

        private static string FormatBitsPerSecond(
            long bitsPerSecond)
        {
            if (bitsPerSecond < 1_000)
                return $"{bitsPerSecond:N0} bps";

            if (bitsPerSecond < 1_000_000)
                return $"{bitsPerSecond / 1_000.0:N1} Kbps";

            if (bitsPerSecond < 1_000_000_000)
                return $"{bitsPerSecond / 1_000_000.0:N1} Mbps";

            return
                $"{bitsPerSecond / 1_000_000_000.0:N1} Gbps";
        }

        private void FilterTcpConnections()
        {
            FilteredTcpConnections.Clear();

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                foreach (var connection in TcpConnections)
                {
                    FilteredTcpConnections.Add(connection);
                }

                return;
            }

            var searchText = SearchText.Trim();

            foreach (var connection in TcpConnections)
            {
                if (MatchesSearch(connection, searchText))
                {
                    FilteredTcpConnections.Add(connection);
                }
            }
        }

        private static bool MatchesSearch(TcpConnection connection, string searchText)
        {
            return
                connection.ProcessName.Contains(
                    searchText,
                    StringComparison.OrdinalIgnoreCase)

                || connection.ProcessId
                    .ToString()
                    .Contains(searchText, StringComparison.OrdinalIgnoreCase)

                || connection.LocalAddress.Contains(
                    searchText,
                    StringComparison.OrdinalIgnoreCase)

                || connection.LocalPort
                    .ToString()
                    .Contains(searchText, StringComparison.OrdinalIgnoreCase)

                || connection.RemoteAddress.Contains(
                    searchText,
                    StringComparison.OrdinalIgnoreCase)

                || connection.RemotePort
                    .ToString()
                    .Contains(searchText, StringComparison.OrdinalIgnoreCase)

                || connection.State
                    .ToString()
                    .Contains(searchText, StringComparison.OrdinalIgnoreCase);
        }
    }
}