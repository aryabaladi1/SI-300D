using SI_300D.ViewModels;
using System.Windows;

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
    }
}