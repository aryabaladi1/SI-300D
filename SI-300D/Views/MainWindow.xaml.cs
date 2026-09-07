using SI_300D.Models;
using SI_300D.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace SI_300D.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainViewModel();
        }

        private async void StartMonitoring_Click(object sender, RoutedEventArgs e)
        {
            await ((MainViewModel)DataContext).StartMonitoringAsync();
        }

        private void StopMonitoring_Click(object sender, RoutedEventArgs e)
        {
            ((MainViewModel)DataContext).StopMonitoring();
        }

        private void InterfaceSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is MainViewModel viewModel &&
                sender is ComboBox comboBox &&
                comboBox.SelectedItem is NetworkInterfaceInfo selectedInterface)
            {
                viewModel.SelectNetworkInterface(selectedInterface);
            }
        }
    }
}