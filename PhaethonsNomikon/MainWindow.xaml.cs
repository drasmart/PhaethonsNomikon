using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace PhaethonsNomikon;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly double _logBoxMinHeight;
    private double _lastLogBoxHeight;
    private readonly ILogger _logger;

    public SaveDocument Document { get; set; } = new(true);
    
    public MainWindow()
    {
        _logger = ((App)Application.Current).ServiceProvider.GetRequiredService<ILogger<MainWindow>>();
        InitializeComponent();
        _logBoxMinHeight = LogContainer.RowDefinitions[2].MinHeight;
        ToggleLogView.IsChecked = false;
    }
    
    private void ToggleLogView_Checked(object sender, RoutedEventArgs e)
    {
        using (_logger.BeginScope("{show-logs}", true))
            _logger.LogInformation("Toggle Logs ON");
        if (LogViewBox != null)
        {
            LogViewBox.Visibility = Visibility.Visible;
        }
        if (LogContainer?.RowDefinitions != null && LogContainer.RowDefinitions.Count >= 3)
        {
            LogContainer.RowDefinitions[1].Height = new GridLength(5); // Show splitter
            // Show log view
            LogContainer.RowDefinitions[2].MinHeight = _logBoxMinHeight;
            LogContainer.RowDefinitions[2].Height = new GridLength(_lastLogBoxHeight);
        }
    }

    private void ToggleLogView_Unchecked(object sender, RoutedEventArgs e)
    {
        using (_logger.BeginScope("{show-logs}", false))
            _logger.LogInformation("Toggle Logs OFF");
        if (LogViewBox != null)
        {
            _lastLogBoxHeight = LogViewBox.ActualHeight;
            LogViewBox.Visibility = Visibility.Collapsed;
        }
        if (LogContainer?.RowDefinitions != null && LogContainer.RowDefinitions.Count >= 3)
        {
            LogContainer.RowDefinitions[1].Height = new GridLength(0); // Hide splitter
            // Collapse log view
            LogContainer.RowDefinitions[2].MinHeight = 0;
            LogContainer.RowDefinitions[2].Height = new GridLength(0);
        }
    }
}